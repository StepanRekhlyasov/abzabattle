using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionController(
    AppDbContext db,
    SessionBroadcastService broadcast,
    SessionUserService sessionUsers,
    SessionActionLogger actionLogger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SessionResponse>> Create([FromBody] CreateSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { detail = "Session name is required" });
        }

        if (request.Faction is not (Faction.Imperial or Faction.Rebel))
        {
            return BadRequest(new { detail = "Invalid faction" });
        }

        if (request.PtsLimit <= 0 || request.MapSize < 8 || request.MapSize > 40)
        {
            return BadRequest(new { detail = "Invalid session settings" });
        }

        var playerName = request.PlayerName.Trim();
        var userExists = await db.Users.AnyAsync(u => u.Name == playerName);
        if (!userExists)
        {
            return NotFound(new { detail = "User not found" });
        }

        var battleMapJson = BattleMapNormalizer.PrepareForStorage(request.BattleMap.GetRawText());
        if (!BattleMapValidator.HasRemainingUnits(battleMapJson))
        {
            return BadRequest(new { detail = "Battle map must contain deployed units" });
        }

        var ptsSpent = BattleMapValidator.CalculatePtsSpent(battleMapJson);
        if (ptsSpent > request.PtsLimit)
        {
            return BadRequest(new { detail = "PTS limit exceeded" });
        }

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Status = SessionStatus.Pending,
            CurrentTurn = Faction.Rebel,
            PtsLimit = request.PtsLimit,
            MapSize = request.MapSize,
            CreatorPlayerName = playerName,
            CreatedAt = DateTime.UtcNow,
        };

        if (request.Faction == Faction.Rebel)
        {
            session.RebelPlayerName = playerName;
            session.RebelBattleMapJson = battleMapJson;
        }
        else
        {
            session.ImperialPlayerName = playerName;
            session.ImperialBattleMapJson = battleMapJson;
        }

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> List([FromQuery] string? playerName)
    {
        var sessions = await db.Sessions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var viewer = string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        var users = await sessionUsers.GetUsersForSessionsAsync(sessions);

        return Ok(sessions.Select(s => SessionMapper.ToResponse(s, viewer, users)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionResponse>> Get(Guid id, [FromQuery] string? playerName)
    {
        var session = await db.Sessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        var viewer = string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        return Ok(await ToResponseAsync(session, viewer));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string adminName)
    {
        if (!AdminAuth.IsAdmin(adminName))
        {
            return Forbid();
        }

        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        var logs = await db.SessionActionLogs.Where(l => l.SessionId == id).ToListAsync();
        db.SessionActionLogs.RemoveRange(logs);
        db.Sessions.Remove(session);
        await db.SaveChangesAsync();
        await broadcast.BroadcastDeletedAsync(id);

        return NoContent();
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IEnumerable<SessionActionLogResponse>>> GetHistory(Guid id)
    {
        var sessionExists = await db.Sessions.AsNoTracking().AnyAsync(s => s.Id == id);
        if (!sessionExists)
        {
            return NotFound(new { detail = "Session not found" });
        }

        var logs = await actionLogger.GetLogsAsync(id);
        return Ok(logs.Select(log => new SessionActionLogResponse
        {
            Id = log.Id,
            Sequence = log.Sequence,
            PlayerName = log.PlayerName,
            ActionKind = log.ActionKind,
            Message = log.Message,
            PayloadJson = log.PayloadJson,
            CreatedAt = log.CreatedAt,
        }));
    }

    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<SessionResponse>> Join(Guid id, [FromBody] JoinSessionRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status == SessionStatus.Finished)
        {
            return Conflict(new { detail = "This session has already finished" });
        }

        if (session.Status == SessionStatus.InProgress)
        {
            return Conflict(new { detail = "Battle is already in progress" });
        }

        if (session.Status != SessionStatus.Pending)
        {
            return Conflict(new { detail = "Session is not open for joining" });
        }

        if (!SessionMapper.IsCreatorDraftComplete(session))
        {
            return Conflict(new { detail = "Creator is still preparing the session" });
        }

        var playerName = request.PlayerName.Trim();
        if (playerName == session.RebelPlayerName || playerName == session.ImperialPlayerName)
        {
            return Conflict(new { detail = "Player is already in this session" });
        }

        var rebelFree = string.IsNullOrWhiteSpace(session.RebelPlayerName);
        var imperialFree = string.IsNullOrWhiteSpace(session.ImperialPlayerName);

        if (!rebelFree && !imperialFree)
        {
            return Conflict(new { detail = "Session is full" });
        }

        var userExists = await db.Users.AnyAsync(u => u.Name == playerName);
        if (!userExists)
        {
            return NotFound(new { detail = "User not found" });
        }

        if (rebelFree)
        {
            session.RebelPlayerName = playerName;
        }
        else if (imperialFree)
        {
            session.ImperialPlayerName = playerName;
        }

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<SessionResponse>> Start(Guid id, [FromBody] StartBattleRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status != SessionStatus.Pending)
        {
            return Conflict(new { detail = "Battle has already started" });
        }

        var playerName = request.PlayerName.Trim();
        if (playerName == session.CreatorPlayerName)
        {
            return Conflict(new { detail = "Only the joining player can start the battle" });
        }

        if (string.IsNullOrWhiteSpace(session.RebelPlayerName) || string.IsNullOrWhiteSpace(session.ImperialPlayerName))
        {
            return Conflict(new { detail = "Session is not full yet" });
        }

        if (playerName != session.RebelPlayerName && playerName != session.ImperialPlayerName)
        {
            return Conflict(new { detail = "Player is not in this session" });
        }

        var battleMapJson = BattleMapNormalizer.PrepareForStorage(request.BattleMap.GetRawText());
        if (!BattleMapValidator.HasRemainingUnits(battleMapJson))
        {
            return BadRequest(new { detail = "Battle map must contain deployed units" });
        }

        var ptsSpent = BattleMapValidator.CalculatePtsSpent(battleMapJson);
        if (ptsSpent > session.PtsLimit)
        {
            return BadRequest(new { detail = "PTS limit exceeded" });
        }

        if (playerName == session.RebelPlayerName)
        {
            session.RebelBattleMapJson = battleMapJson;
        }
        else
        {
            session.ImperialBattleMapJson = battleMapJson;
        }

        if (!string.IsNullOrWhiteSpace(session.RebelBattleMapJson))
        {
            session.RebelBattleMapJson = BattleMapNormalizer.PrepareForStorage(session.RebelBattleMapJson);
        }

        if (!string.IsNullOrWhiteSpace(session.ImperialBattleMapJson))
        {
            session.ImperialBattleMapJson = BattleMapNormalizer.PrepareForStorage(session.ImperialBattleMapJson);
        }

        session.Status = SessionStatus.InProgress;
        session.CurrentTurn = Faction.Rebel;
        session.HitsThisTurn = 0;

        await actionLogger.LogBattleStartAsync(session);

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    [HttpPost("{id:guid}/attack")]
    public async Task<ActionResult<SessionResponse>> Attack(Guid id, [FromBody] AttackSectorRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status != SessionStatus.InProgress)
        {
            return Conflict(new { detail = "Battle is not in progress" });
        }

        var playerName = request.PlayerName.Trim();
        var isRebel = playerName == session.RebelPlayerName;
        var isImperial = playerName == session.ImperialPlayerName;

        if (!isRebel && !isImperial)
        {
            return Conflict(new { detail = "Player is not in this session" });
        }

        var playerFaction = isRebel ? Faction.Rebel : Faction.Imperial;
        if (session.CurrentTurn != playerFaction)
        {
            return Conflict(new { detail = "Not your turn" });
        }

        var opponentMapJson = isRebel ? session.ImperialBattleMapJson : session.RebelBattleMapJson;
        if (string.IsNullOrWhiteSpace(opponentMapJson))
        {
            return BadRequest(new { detail = "Opponent battle map is not available" });
        }

        if (!BattleMapMutator.TryStrikeSector(opponentMapJson, request.X, request.Y, out var updatedMapJson, out var outcome))
        {
            return BadRequest(new { detail = "Sector cannot be attacked" });
        }

        if (isRebel)
        {
            session.ImperialBattleMapJson = updatedMapJson;
        }
        else
        {
            session.RebelBattleMapJson = updatedMapJson;
        }

        var turnBefore = session.CurrentTurn;
        var shotNumber = session.HitsThisTurn + 1;
        var targetBefore = BattleMapReader.TryGetSectorTarget(opponentMapJson, request.X, request.Y);

        await CheckEndGameConditions(session, updatedMapJson, playerName);
        BattleTurnRules.ApplyNormalAttackOutcome(session, outcome);
        var endedTurn = session.CurrentTurn != turnBefore;

        await actionLogger.LogAttackAsync(
            session,
            playerName,
            request.X,
            request.Y,
            shotNumber,
            targetBefore,
            outcome,
            updatedMapJson,
            endedTurn);

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    [HttpPost("{id:guid}/ability")]
    public async Task<ActionResult<SessionResponse>> UseAbility(Guid id, [FromBody] UseAbilityRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status != SessionStatus.InProgress)
        {
            return Conflict(new { detail = "Battle is not in progress" });
        }

        var playerName = request.PlayerName.Trim();
        var isRebel = playerName == session.RebelPlayerName;
        var isImperial = playerName == session.ImperialPlayerName;

        if (!isRebel && !isImperial)
        {
            return Conflict(new { detail = "Player is not in this session" });
        }

        var playerFaction = isRebel ? Faction.Rebel : Faction.Imperial;
        if (session.CurrentTurn != playerFaction)
        {
            return Conflict(new { detail = "Not your turn" });
        }

        var ownMapJson = isRebel ? session.RebelBattleMapJson : session.ImperialBattleMapJson;
        var opponentMapJson = isRebel ? session.ImperialBattleMapJson : session.RebelBattleMapJson;

        if (string.IsNullOrWhiteSpace(opponentMapJson) || string.IsNullOrWhiteSpace(ownMapJson)) {
            return BadRequest(new { detail = "Battle map is not available" });
        }

        switch (request.AbilityKind)
        {
            case "deploy-tie-fighter":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(ownMapJson, request.X, request.Y);

                if (!BattleMapMutator.TryDeployTieFighter(ownMapJson, request.X, request.Y, out var updatedOwnMapJson))
                {
                    return BadRequest(new { detail = "Sector cannot be used for deployment" });
                }

                session.ImperialBattleMapJson = updatedOwnMapJson;
                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    null,
                    updatedOwnMapJson,
                    isRebel);

                break;
            }
            case "place-space-mine":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(ownMapJson, request.X, request.Y);

                if (!BattleMapMutator.TryDeploySpaceMine(ownMapJson, request.X, request.Y, out var updatedOwnMapJson))
                {
                    return BadRequest(new { detail = "Sector cannot be used for mine placement" });
                }

                session.ImperialBattleMapJson = updatedOwnMapJson;
                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    null,
                    updatedOwnMapJson,
                    isRebel);

                break;
            }
            case "opponent-strike":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(opponentMapJson, request.X, request.Y);

                if (!BattleMapMutator.TryStrikeSector(opponentMapJson, request.X, request.Y, out var updatedMapJson, out var outcome))
                {
                    return BadRequest(new { detail = "Sector cannot be attacked" });
                }

                if (isRebel)
                {
                    session.ImperialBattleMapJson = updatedMapJson;
                }
                else
                {
                    session.RebelBattleMapJson = updatedMapJson;
                }

                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    outcome,
                    updatedMapJson,
                    isRebel);

                if (outcome == StrikeOutcome.MineHit)
                {
                    BattleTurnRules.EndTurn(session);
                }

                break;
            }
            case "one-in-a-million":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(opponentMapJson, request.X, request.Y);

                if (!BattleMapMutator.TryOneInAMillionStrike(
                        opponentMapJson,
                        request.X,
                        request.Y,
                        out var updatedMapJson,
                        out var outcome))
                {
                    return BadRequest(new { detail = "Sector cannot be attacked" });
                }

                if (isRebel)
                {
                    session.ImperialBattleMapJson = updatedMapJson;
                }
                else
                {
                    session.RebelBattleMapJson = updatedMapJson;
                }

                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    outcome,
                    updatedMapJson,
                    isRebel);

                if (outcome == StrikeOutcome.MineHit)
                {
                    BattleTurnRules.EndTurn(session);
                }

                break;
            }
            case "bombardment":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(opponentMapJson, request.X, request.Y);

                if (!BattleMapMutator.TryBombardmentStrike(
                        opponentMapJson,
                        request.X,
                        request.Y,
                        out var updatedMapJson,
                        out var outcome))
                {
                    return BadRequest(new { detail = "Sector cannot be attacked" });
                }

                if (isRebel)
                {
                    session.ImperialBattleMapJson = updatedMapJson;
                }
                else
                {
                    session.RebelBattleMapJson = updatedMapJson;
                }

                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    outcome,
                    updatedMapJson,
                    isRebel);

                if (outcome == StrikeOutcome.MineHit)
                {
                    BattleTurnRules.EndTurn(session);
                }

                break;
            }
            case "single-reactor-ignition":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(opponentMapJson, request.X, request.Y);

                if (!BattleMapMutator.TrySingleReactorIgnitionStrike(
                        opponentMapJson,
                        request.X,
                        request.Y,
                        out var updatedMapJson,
                        out var outcome))
                {
                    return BadRequest(new { detail = "Sector cannot be attacked" });
                }

                if (isRebel)
                {
                    session.ImperialBattleMapJson = updatedMapJson;
                }
                else
                {
                    session.RebelBattleMapJson = updatedMapJson;
                }

                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    outcome,
                    updatedMapJson,
                    isRebel);

                if (outcome == StrikeOutcome.MineHit)
                {
                    BattleTurnRules.EndTurn(session);
                }

                break;
            }
            case "place-shield":
            {
                var targetBefore = BattleMapReader.TryGetSectorTarget(ownMapJson, request.X, request.Y);

                if (string.IsNullOrWhiteSpace(request.SourceEntityId))
                {
                    return BadRequest(new { detail = "Source entity is required" });
                }

                if (!BattleMapMutator.TryPlaceShield(
                        ownMapJson,
                        request.X,
                        request.Y,
                        request.SourceEntityId.Trim(),
                        out var updatedOwnMapJson))
                {
                    return BadRequest(new { detail = "Sector cannot be shielded" });
                }

                session.RebelBattleMapJson = updatedOwnMapJson;
                await actionLogger.LogAbilityAsync(
                    session,
                    playerName,
                    request.AbilityKind,
                    request.X,
                    request.Y,
                    targetBefore,
                    null,
                    updatedOwnMapJson,
                    isRebel);

                break;
            }
            default:
                return BadRequest(new { detail = "Unknown ability" });
        }

        var opponentMapAfterAbility = isRebel ? session.ImperialBattleMapJson : session.RebelBattleMapJson;
        if (!string.IsNullOrWhiteSpace(opponentMapAfterAbility))
        {
            await CheckEndGameConditions(session, opponentMapAfterAbility, playerName);
        }

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    [HttpPost("{id:guid}/give-up")]
    public async Task<ActionResult<SessionResponse>> GiveUp(Guid id, [FromBody] GiveUpRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status != SessionStatus.InProgress)
        {
            return Conflict(new { detail = "Battle is not in progress" });
        }

        var playerName = request.PlayerName.Trim();
        var isRebel = playerName == session.RebelPlayerName;
        var isImperial = playerName == session.ImperialPlayerName;

        if (!isRebel && !isImperial)
        {
            return Conflict(new { detail = "Player is not in this session" });
        }

        var winnerName = isRebel ? session.ImperialPlayerName! : session.RebelPlayerName!;

        await actionLogger.LogGiveUpAsync(session, playerName);
        await AwardVictoryAsync(session, winnerName, playerName);

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(await ToResponseAsync(session, playerName));
    }

    private async Task<SessionResponse> ToResponseAsync(GameSession session, string? viewer)
    {
        var users = await sessionUsers.GetUsersForSessionAsync(session);
        return SessionMapper.ToResponse(session, viewer, users);
    }

    private async Task CheckEndGameConditions(GameSession session, string battleMapJson, string playerName) {
        if (session.Status == SessionStatus.Finished)
        {
            return;
        }

        if (BattleMapValidator.HasRemainingUnits(battleMapJson))
        {
            return;
        }

        var isRebel = playerName == session.RebelPlayerName;
        var isImperial = playerName == session.ImperialPlayerName;

        if (!isRebel && !isImperial)
        {
            throw new Exception("Player is not in this session");
        }

        var loserName = isRebel ? session.ImperialPlayerName! : session.RebelPlayerName!;
        await AwardVictoryAsync(session, playerName, loserName);
    }

    private async Task AwardVictoryAsync(GameSession session, string winnerName, string loserName)
    {
        var winner = await db.Users.FirstAsync(u => u.Name == winnerName);
        var loser = await db.Users.FirstAsync(u => u.Name == loserName);

        session.Status = SessionStatus.Finished;
        session.WinnerPlayerName = winnerName;

        winner.Wins++;
        winner.TotalGames++;
        loser.Loses++;
        loser.TotalGames++;
    }
}
