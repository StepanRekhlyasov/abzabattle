using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapReader
{
    public static SectorTarget? TryGetSectorTarget(string battleMapJson, int x, int y)
    {
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return null;
        }

        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return null;
        }

        var entity = sector["entity"];
        return new SectorTarget(
            entity?["type"]?.GetValue<string>(),
            entity?["id"]?.GetValue<string>(),
            sector["destroyed"]?.GetValue<bool>() ?? false,
            sector["hidden"]?.GetValue<bool>() ?? false,
            sector["shielded"]?.GetValue<bool>() ?? false);
    }

    public static bool HasIntactEntity(string battleMapJson, string entityId)
    {
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        for (var rowY = 0; rowY < sectors.Count; rowY++)
        {
            if (sectors[rowY] is not JsonArray row)
            {
                continue;
            }

            for (var rowX = 0; rowX < row.Count; rowX++)
            {
                if (row[rowX] is not JsonObject sectorObject)
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

public sealed record SectorTarget(
    string? EntityType,
    string? EntityId,
    bool Destroyed,
    bool Hidden,
    bool Shielded)
{
    public bool IsEmpty => EntityType is null or "empty";
}
