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
    SessionBroadcastService broadcast) : ControllerBase
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

        var battleMapJson = request.BattleMap.GetRawText();
        if (!BattleMapValidator.HasDeployedUnits(battleMapJson))
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

        return Ok(SessionMapper.ToResponse(session, playerName));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> List([FromQuery] string? playerName)
    {
        var sessions = await db.Sessions
            .AsNoTracking()
            .Where(s => s.Status == SessionStatus.Pending || s.Status == SessionStatus.InProgress)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        var viewer = string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        return Ok(sessions.Select(s => SessionMapper.ToResponse(s, viewer)));
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
        return Ok(SessionMapper.ToResponse(session, viewer));
    }

    [HttpPost("{id:guid}/join")]
    public async Task<ActionResult<SessionResponse>> Join(Guid id, [FromBody] JoinSessionRequest request)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
        {
            return NotFound(new { detail = "Session not found" });
        }

        if (session.Status != SessionStatus.Pending)
        {
            return Conflict(new { detail = "Session is not open for joining" });
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

        return Ok(SessionMapper.ToResponse(session, playerName));
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

        var battleMapJson = request.BattleMap.GetRawText();
        if (!BattleMapValidator.HasDeployedUnits(battleMapJson))
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

        session.Status = SessionStatus.InProgress;
        session.CurrentTurn = Faction.Rebel;

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(SessionMapper.ToResponse(session, playerName));
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

        if (!BattleMapMutator.TryStrikeSector(opponentMapJson, request.X, request.Y, out var updatedMapJson, out var isHit))
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

        if (!isHit)
        {
            session.CurrentTurn = Faction.Opposite(playerFaction);
        }

        await db.SaveChangesAsync();
        await broadcast.BroadcastUpdatedAsync(session);

        return Ok(SessionMapper.ToResponse(session, playerName));
    }
}
