using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapPlacement
{
    public static bool CanPlaceWithEmptyAdjacentCells(JsonArray sectors, int x, int y)
    {
        if (!IsSectorEmpty(sectors, x, y))
        {
            return false;
        }

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

                if (!IsInsideMap(sectors, adjacentX, adjacentY))
                {
                    continue;
                }

                if (!IsSectorEmpty(sectors, adjacentX, adjacentY))
                {
                    return false;
                }
            }
        }

        return true;
    }


    private static bool IsSectorEmpty(JsonArray sectors, int x, int y)
    {
        if (!TryGetSector(sectors, x, y, out var sector))
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        return entityType == "empty";
    }

    private static bool IsInsideMap(JsonArray sectors, int x, int y)
    {
        if (y < 0 || y >= sectors.Count)
        {
            return false;
        }

        if (sectors[y] is not JsonArray row)
        {
            return false;
        }

        return x >= 0 && x < row.Count;
    }

    private static bool TryGetSector(JsonArray sectors, int x, int y, out JsonObject sector)
    {
        sector = null!;

        if (!IsInsideMap(sectors, x, y))
        {
            return false;
        }

        var row = (JsonArray)sectors[y]!;
        if (row[x] is not JsonObject sectorObject)
        {
            return false;
        }

        sector = sectorObject;
        return true;
    }
}
