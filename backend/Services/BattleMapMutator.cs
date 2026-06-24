using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapMutator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static bool TryStrikeSector(
        string battleMapJson,
        int x,
        int y,
        out string updatedJson,
        out StrikeOutcome outcome)
    {
        updatedJson = battleMapJson;
        outcome = StrikeOutcome.Miss;

        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;
        if (destroyed)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        var isUnit = entityType is not null and not "empty";
        var shielded = sector["shielded"]?.GetValue<bool>() ?? false;
        var shieldRevealed = sector["shieldRevealed"]?.GetValue<bool>() ?? false;

        if (shielded && isUnit)
        {
            if (!shieldRevealed)
            {
                sector["hidden"] = false;
                sector["shieldRevealed"] = true;
                outcome = StrikeOutcome.ShieldReveal;
                updatedJson = root.ToJsonString(JsonOptions);
                return true;
            }

            sector["shielded"] = false;
            sector["shieldRevealed"] = true;
            sector["hidden"] = false;
            sector["destroyed"] = false;
            outcome = StrikeOutcome.ShieldBreak;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        outcome = isUnit ? StrikeOutcome.Hit : StrikeOutcome.Miss;
        sector["hidden"] = false;
        sector["destroyed"] = true;
        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    public static bool TryAirborneSuperiorityStrike(
        string battleMapJson,
        int x,
        int y,
        out string updatedJson,
        out StrikeOutcome outcome)
    {
        updatedJson = battleMapJson;
        outcome = StrikeOutcome.Miss;

        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;
        if (destroyed)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType != "tie-fighter")
        {
            sector["hidden"] = false;
            outcome = StrikeOutcome.Miss;
            if (entityType is null or "empty")
            {
                sector["destroyed"] = true;
            }

            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        var shielded = sector["shielded"]?.GetValue<bool>() ?? false;
        var shieldRevealed = sector["shieldRevealed"]?.GetValue<bool>() ?? false;

        if (shielded)
        {
            if (!shieldRevealed)
            {
                sector["hidden"] = false;
                sector["shieldRevealed"] = true;
                outcome = StrikeOutcome.ShieldReveal;
                updatedJson = root.ToJsonString(JsonOptions);
                return true;
            }

            sector["shielded"] = false;
            sector["shieldRevealed"] = true;
            sector["hidden"] = false;
            sector["destroyed"] = false;
            outcome = StrikeOutcome.ShieldBreak;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        outcome = StrikeOutcome.Hit;
        sector["hidden"] = false;
        sector["destroyed"] = true;
        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    public static bool TryBombardmentStrike(
        string battleMapJson,
        int x,
        int y,
        out string updatedJson,
        out StrikeOutcome outcome)
    {
        updatedJson = battleMapJson;
        outcome = StrikeOutcome.Miss;

        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;
        if (destroyed)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType == "tie-fighter")
        {
            sector["hidden"] = false;
            outcome = StrikeOutcome.Miss;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        var isUnit = entityType is not null and not "empty";
        if (!isUnit)
        {
            sector["hidden"] = false;
            sector["destroyed"] = true;
            outcome = StrikeOutcome.Miss;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        var shielded = sector["shielded"]?.GetValue<bool>() ?? false;
        var shieldRevealed = sector["shieldRevealed"]?.GetValue<bool>() ?? false;

        if (shielded && isUnit)
        {
            if (!shieldRevealed)
            {
                sector["hidden"] = false;
                sector["shieldRevealed"] = true;
                outcome = StrikeOutcome.ShieldReveal;
                updatedJson = root.ToJsonString(JsonOptions);
                return true;
            }

            sector["shielded"] = false;
            sector["shieldRevealed"] = true;
            sector["hidden"] = false;
            sector["destroyed"] = false;
            outcome = StrikeOutcome.ShieldBreak;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        outcome = isUnit ? StrikeOutcome.Hit : StrikeOutcome.Miss;
        sector["hidden"] = false;
        sector["destroyed"] = true;
        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    public static bool TryDeployTieFighter(string battleMapJson, int x, int y, out string updatedJson)
    {
        updatedJson = battleMapJson;
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType is not "empty")
        {
            return false;
        }

        if (!BattleMapPlacement.CanPlaceTieFighter(sectors, x, y))
        {
            return false;
        }

        sector["entity"] = new JsonObject
        {
            ["type"] = "tie-fighter",
            ["id"] = Guid.NewGuid().ToString(),
            ["content"] = "TF",
            ["rotation"] = 0,
        };
        sector["destroyed"] = false;

        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    public static bool TryPlaceShield(
        string battleMapJson,
        int x,
        int y,
        string sourceEntityId,
        out string updatedJson)
    {
        updatedJson = battleMapJson;
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;
        if (destroyed)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType is null or "empty" or "nebulon-frigate")
        {
            return false;
        }

        var entityId = sector["entity"]?["id"]?.GetValue<string>();
        if (entityId is not null && entityId == sourceEntityId)
        {
            return false;
        }

        var shielded = sector["shielded"]?.GetValue<bool>() ?? false;
        if (shielded)
        {
            return false;
        }

        sector["shielded"] = true;
        sector["shieldRevealed"] = false;

        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    private static bool TryGetSector(JsonArray sectors, int x, int y, out JsonObject sector)
    {
        sector = null!;
        if (y < 0 || y >= sectors.Count)
        {
            return false;
        }

        if (sectors[y] is not JsonArray row || x < 0 || x >= row.Count)
        {
            return false;
        }

        if (row[x] is not JsonObject sectorObject)
        {
            return false;
        }

        sector = sectorObject;
        return true;
    }
}
