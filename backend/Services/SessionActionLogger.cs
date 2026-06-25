using System.Text.Json;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SessionActionLogger(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task LogAttackAsync(
        GameSession session,
        string playerName,
        int x,
        int y,
        int shotNumber,
        SectorTarget? targetBefore,
        StrikeOutcome outcome,
        string updatedOpponentMapJson,
        bool endedTurn,
        CancellationToken cancellationToken = default)
    {
        var isKill = IsKill(targetBefore, outcome, updatedOpponentMapJson);
        var message = FormatAttackMessage(playerName, x, y, shotNumber, targetBefore, outcome, isKill, endedTurn);
        var payload = new
        {
            actionKind = "attack",
            x,
            y,
            shotNumber,
            outcome = outcome.ToString(),
            targetEntityType = targetBefore?.EntityType,
            targetEntityId = targetBefore?.EntityId,
            targetEntityDisplayName = GetEntityDisplayName(targetBefore?.EntityType),
            isKill,
            endedTurn,
            snapshot = BuildSnapshot(session),
        };

        await AddLogAsync(session.Id, playerName, "attack", message, payload, cancellationToken);
    }

    public async Task LogAbilityAsync(
        GameSession session,
        string playerName,
        string abilityKind,
        int? x,
        int? y,
        SectorTarget? targetBefore,
        StrikeOutcome? outcome,
        string? updatedMapJson,
        bool isRebel,
        CancellationToken cancellationToken = default)
    {
        var abilityName = GetAbilityName(abilityKind, isRebel);
        var isKill = outcome.HasValue && updatedMapJson is not null
            && IsKill(targetBefore, outcome.Value, updatedMapJson);
        var message = FormatAbilityMessage(
            playerName,
            abilityKind,
            abilityName,
            x,
            y,
            targetBefore,
            outcome,
            isKill);
        var payload = new
        {
            actionKind = abilityKind,
            abilityName,
            x,
            y,
            outcome = outcome?.ToString(),
            targetEntityType = targetBefore?.EntityType,
            targetEntityId = targetBefore?.EntityId,
            targetEntityDisplayName = GetEntityDisplayName(targetBefore?.EntityType),
            isKill,
            endedTurn = false,
            snapshot = BuildSnapshot(session),
        };

        await AddLogAsync(session.Id, playerName, abilityKind, message, payload, cancellationToken);
    }

    public async Task LogBattleStartAsync(
        GameSession session,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            actionKind = "battle-start",
            snapshot = BuildSnapshot(session),
        };

        await AddLogAsync(
            session.Id,
            "system",
            "battle-start",
            "Battle started.",
            payload,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SessionActionLog>> GetLogsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.SessionActionLogs
            .AsNoTracking()
            .Where(log => log.SessionId == sessionId)
            .OrderBy(log => log.Sequence)
            .ToListAsync(cancellationToken);
    }

    private async Task AddLogAsync(
        Guid sessionId,
        string playerName,
        string actionKind,
        string message,
        object payload,
        CancellationToken cancellationToken)
    {
        var sequence = await db.SessionActionLogs
            .Where(log => log.SessionId == sessionId)
            .Select(log => (int?)log.Sequence)
            .MaxAsync(cancellationToken) ?? 0;

        db.SessionActionLogs.Add(new SessionActionLog
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Sequence = sequence + 1,
            PlayerName = playerName,
            ActionKind = actionKind,
            Message = message,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            CreatedAt = DateTime.UtcNow,
        });
    }

    private static object BuildSnapshot(GameSession session) => new
    {
        rebelBattleMap = ParseMapJson(session.RebelBattleMapJson),
        imperialBattleMap = ParseMapJson(session.ImperialBattleMapJson),
        currentTurn = session.CurrentTurn,
        hitsThisTurn = session.HitsThisTurn,
    };

    private static object? ParseMapJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    private static bool IsKill(SectorTarget? targetBefore, StrikeOutcome outcome, string mapJson)
    {
        if (targetBefore?.EntityId is not { Length: > 0 } entityId)
        {
            return false;
        }

        if (outcome != StrikeOutcome.Hit)
        {
            return false;
        }

        return !BattleMapReader.HasIntactEntity(mapJson, entityId);
    }

    private static string FormatAttackMessage(
        string playerName,
        int x,
        int y,
        int shotNumber,
        SectorTarget? targetBefore,
        StrikeOutcome outcome,
        bool isKill,
        bool endedTurn)
    {
        var shotLabel = GetShotLabel(shotNumber);
        var suffix = endedTurn ? " End of turn." : string.Empty;

        if (outcome == StrikeOutcome.Miss)
        {
            return $"{playerName} attacks {x}/{y} sector with {shotLabel} shot and misses.{suffix}";
        }

        if (outcome == StrikeOutcome.ShieldBreak)
        {
            var entityName = GetEntityDisplayName(targetBefore?.EntityType);
            return $"{playerName} attacks {x}/{y} sector with {shotLabel} shot and breaks shield on {entityName}!{suffix}";
        }

        var hitEntityName = GetEntityDisplayName(targetBefore?.EntityType);
        if (isKill)
        {
            return $"{playerName} attacks {x}/{y} sector with {shotLabel} shot and hit {hitEntityName} - it's a kill!{suffix}";
        }

        return $"{playerName} attacks {x}/{y} sector with {shotLabel} shot and hit {hitEntityName}!{suffix}";
    }

    private static string FormatAbilityMessage(
        string playerName,
        string abilityKind,
        string abilityName,
        int? x,
        int? y,
        SectorTarget? targetBefore,
        StrikeOutcome? outcome,
        bool isKill)
    {
        return abilityKind switch
        {
            "place-shield" => $"{playerName} uses {abilityName}.",
            "deploy-tie-fighter" => $"{playerName} uses {abilityName} on {x}/{y} sector.",
            _ => FormatTargetedAbilityMessage(playerName, abilityName, x, y, targetBefore, outcome, isKill),
        };
    }

    private static string FormatTargetedAbilityMessage(
        string playerName,
        string abilityName,
        int? x,
        int? y,
        SectorTarget? targetBefore,
        StrikeOutcome? outcome,
        bool isKill)
    {
        var coordinates = x.HasValue && y.HasValue ? $" on {x}/{y} sector" : string.Empty;

        if (outcome == StrikeOutcome.ShieldBreak)
        {
            var entityName = GetEntityDisplayName(targetBefore?.EntityType);
            return $"{playerName} uses {abilityName}{coordinates} and breaks shield on {entityName}!";
        }

        if (outcome == StrikeOutcome.Hit)
        {
            var entityName = GetEntityDisplayName(targetBefore?.EntityType);
            if (isKill)
            {
                return $"{playerName} uses {abilityName}{coordinates} and hit {entityName} - it's a kill!";
            }

            return $"{playerName} uses {abilityName}{coordinates} and hit {entityName}!";
        }

        if (targetBefore is not null && !targetBefore.IsEmpty && outcome == StrikeOutcome.Miss)
        {
            var entityName = GetEntityDisplayName(targetBefore.EntityType);
            return $"{playerName} uses {abilityName}{coordinates} and reveals {entityName}.";
        }

        return $"{playerName} uses {abilityName}{coordinates} and misses.";
    }

    private static string GetShotLabel(int shotNumber) => shotNumber switch
    {
        1 => "first",
        2 => "second",
        3 => "third",
        _ => $"{shotNumber}th",
    };

    private static string GetAbilityName(string abilityKind, bool isRebel) => abilityKind switch
    {
        "deploy-tie-fighter" => "Tie Fighter Reinforcement",
        "place-shield" => "Deflector Shield",
        "airborne-superiority" => "Airborne Superiority",
        "bombardment" => "Bombardment",
        "opponent-strike" => isRebel ? "Turbolaser Batteries" : "Swarm Tactics",
        _ => abilityKind,
    };

    private static string GetEntityDisplayName(string? entityType) => entityType switch
    {
        "star-destroyer" => "Star Destroyer",
        "mon-calamari" => "Mon Calamari",
        "tie-fighter" => "Tie Fighter",
        "nebulon-frigate" => "Nebulon Frigate",
        "x-wing" => "X-Wing",
        "u-wing" => "U-Wing",
        "empty" or null => "empty sector",
        _ => entityType,
    };
}
