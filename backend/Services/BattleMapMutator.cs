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

        return ApplySectorHit(sectors, sector, out updatedJson, out outcome, root);
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

        return ApplySectorHit(sectors, sector, out updatedJson, out outcome, root);
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

        return ApplySectorHit(sectors, sector, out updatedJson, out outcome, root);
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

    private static bool ApplySectorHit(
        JsonArray sectors,
        JsonObject sector,
        out string updatedJson,
        out StrikeOutcome outcome,
        JsonNode root)
    {
        var entityType = sector["entity"]?["type"]?.GetValue<string>();
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
        var isHidden = sector["hidden"]?.GetValue<bool>() ?? false;

        if (shielded && isUnit)
        {
            if (isHidden)
            {
                sector["hidden"] = false;
                outcome = StrikeOutcome.Hit;
                updatedJson = root.ToJsonString(JsonOptions);
                return true;
            }

            sector["shielded"] = false;
            sector["hidden"] = false;
            sector["destroyed"] = false;
            outcome = StrikeOutcome.ShieldBreak;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        outcome = StrikeOutcome.Hit;
        sector["hidden"] = false;
        sector["destroyed"] = true;

        var entityId = sector["entity"]?["id"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(entityId) && !HasIntactEntitySectors(sectors, entityId))
        {
            MarkAdjacentSectorsDestroyed(sectors, entityId);
        }

        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    private static bool HasIntactEntitySectors(JsonArray sectors, string entityId)
    {
        for (var y = 0; y < sectors.Count; y++)
        {
            if (sectors[y] is not JsonArray row)
            {
                continue;
            }

            for (var x = 0; x < row.Count; x++)
            {
                if (row[x] is not JsonObject sectorObject)
                {
                    continue;
                }

                if (sectorObject["entity"]?["id"]?.GetValue<string>() != entityId)
                {
                    continue;
                }

                var destroyed = sectorObject["destroyed"]?.GetValue<bool>() ?? false;
                if (!destroyed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void MarkAdjacentSectorsDestroyed(JsonArray sectors, string entityId)
    {
        var entityPositions = new HashSet<string>();
        var footprint = new List<(int X, int Y)>();

        for (var y = 0; y < sectors.Count; y++)
        {
            if (sectors[y] is not JsonArray row)
            {
                continue;
            }

            for (var x = 0; x < row.Count; x++)
            {
                if (row[x] is not JsonObject sectorObject)
                {
                    continue;
                }

                if (sectorObject["entity"]?["id"]?.GetValue<string>() != entityId)
                {
                    continue;
                }

                footprint.Add((x, y));
                entityPositions.Add($"{x},{y}");
            }
        }

        var adjacentPositions = new HashSet<(int X, int Y)>();
        foreach (var (x, y) in footprint)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var adjacentX = x + dx;
                    var adjacentY = y + dy;
                    if (entityPositions.Contains($"{adjacentX},{adjacentY}"))
                    {
                        continue;
                    }

                    adjacentPositions.Add((adjacentX, adjacentY));
                }
            }
        }

        foreach (var (x, y) in adjacentPositions)
        {
            if (!TryGetSector(sectors, x, y, out var adjacentSector))
            {
                continue;
            }

            adjacentSector["hidden"] = false;
            adjacentSector["destroyed"] = true;
        }
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
