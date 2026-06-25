using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapMutator
{
    private const int DeathStarReactorDestroyChancePercent = 33;

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

        if (TryHandleSpaceMineHit(sector, root, out updatedJson, out outcome))
        {
            return true;
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

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;

        if (destroyed && !IsDeathStarSector(sector))
        {
            return false;
        }

        if (!destroyed && TryHandleSpaceMineHit(sector, root, out updatedJson, out outcome))
        {
            return true;
        }

        if (TryReactorCriticalStrike(sectors, sector, root, out updatedJson, out outcome))
        {
            return true;
        }

        if (IsDeathStarSector(sector) && destroyed)
        {
            sector["hidden"] = false;
            outcome = StrikeOutcome.Miss;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

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

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;

        if (destroyed && !IsDeathStarSector(sector))
        {
            return false;
        }

        if (!destroyed && TryHandleSpaceMineHit(sector, root, out updatedJson, out outcome))
        {
            return true;
        }

        if (IsDeathStarSector(sector) && destroyed)
        {
            sector["hidden"] = false;
            outcome = StrikeOutcome.Miss;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        if (entityType == "tie-fighter")
        {
            sector["hidden"] = false;
            outcome = StrikeOutcome.Miss;
            updatedJson = root.ToJsonString(JsonOptions);
            return true;
        }

        return ApplySectorHit(sectors, sector, out updatedJson, out outcome, root);
    }

    public static bool TrySingleReactorIgnitionStrike(
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

        if (!TryGetSector(sectors, x, y, out var targetSector))
        {
            return false;
        }

        if (targetSector["destroyed"]?.GetValue<bool>() ?? false)
        {
            return false;
        }

        var blastPositions = GetBlastPositions(sectors, x, y);
        var affectedEntityIds = new HashSet<string>();
        var hitAnyUnit = false;
        var mineHit = false;

        foreach (var (blastX, blastY) in blastPositions)
        {
            if (!TryGetSector(sectors, blastX, blastY, out var sector))
            {
                continue;
            }

            if (sector["destroyed"]?.GetValue<bool>() ?? false)
            {
                continue;
            }

            var entityType = sector["entity"]?["type"]?.GetValue<string>();
            var entityId = sector["entity"]?["id"]?.GetValue<string>();

            sector["hidden"] = false;

            if (entityType == "space-mine")
            {
                sector["destroyed"] = true;
                mineHit = true;
                continue;
            }

            if (entityType is null or "empty")
            {
                sector["destroyed"] = true;
                continue;
            }

            var shielded = sector["shielded"]?.GetValue<bool>() ?? false;
            if (shielded)
            {
                sector["shielded"] = false;
                hitAnyUnit = true;
                if (entityId is not null)
                {
                    affectedEntityIds.Add(entityId);
                }

                continue;
            }

            sector["destroyed"] = true;
            hitAnyUnit = true;
            if (entityId is not null)
            {
                affectedEntityIds.Add(entityId);
            }
        }

        foreach (var entityId in affectedEntityIds)
        {
            if (!HasIntactEntitySectors(sectors, entityId))
            {
                MarkAdjacentSectorsDestroyed(sectors, entityId);
            }
        }

        if (mineHit)
        {
            outcome = StrikeOutcome.MineHit;
        }
        else if (hitAnyUnit)
        {
            outcome = StrikeOutcome.Hit;
        }

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

        if (!BattleMapPlacement.CanPlaceWithEmptyAdjacentCells(sectors, x, y))
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

    public static bool TryDeploySpaceMine(string battleMapJson, int x, int y, out string updatedJson)
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

        if (!BattleMapPlacement.CanPlaceWithEmptyAdjacentCells(sectors, x, y))
        {
            return false;
        }

        sector["entity"] = new JsonObject
        {
            ["type"] = "space-mine",
            ["id"] = Guid.NewGuid().ToString(),
            ["content"] = "SM",
            ["rotation"] = 0,
        };
        sector["destroyed"] = false;
        sector["hidden"] = true;

        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    private static bool IsDeathStarSector(JsonObject sector) =>
        sector["entity"]?["type"]?.GetValue<string>() == "death-star";

    private static bool IsReactorSector(JsonObject sector) =>
        sector["entity"]?["reactor"]?.GetValue<bool>() ?? false;

    private static bool TryReactorCriticalStrike(
        JsonArray sectors,
        JsonObject sector,
        JsonNode root,
        out string updatedJson,
        out StrikeOutcome outcome)
    {
        updatedJson = root.ToJsonString(JsonOptions);
        outcome = StrikeOutcome.Miss;

        if (!IsDeathStarSector(sector) || !IsReactorSector(sector))
        {
            return false;
        }

        sector["hidden"] = false;

        if (Random.Shared.Next(100) >= DeathStarReactorDestroyChancePercent)
        {
            return false;
        }

        var entityId = sector["entity"]?["id"]?.GetValue<string>();
        if (entityId is null)
        {
            return false;
        }

        DestroyEntireEntity(sectors, entityId);
        outcome = StrikeOutcome.Hit;
        updatedJson = root.ToJsonString(JsonOptions);
        return true;
    }

    private static void DestroyEntireEntity(JsonArray sectors, string entityId)
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

                sectorObject["hidden"] = false;
                sectorObject["destroyed"] = true;
            }
        }

        MarkAdjacentSectorsDestroyed(sectors, entityId);
    }

    private static bool TryHandleSpaceMineHit(
        JsonObject sector,
        JsonNode root,
        out string updatedJson,
        out StrikeOutcome outcome)
    {
        updatedJson = root.ToJsonString(JsonOptions);
        outcome = StrikeOutcome.Miss;

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType != "space-mine")
        {
            return false;
        }

        sector["hidden"] = false;
        sector["destroyed"] = true;
        outcome = StrikeOutcome.MineHit;
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

    private static List<(int X, int Y)> GetBlastPositions(JsonArray sectors, int centerX, int centerY)
    {
        var positions = new List<(int X, int Y)>();

        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                var x = centerX + dx;
                var y = centerY + dy;
                if (TryGetSector(sectors, x, y, out _))
                {
                    positions.Add((x, y));
                }
            }
        }

        return positions;
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
