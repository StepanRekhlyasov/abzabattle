using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapMutator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static bool TryStrikeSector(string battleMapJson, int x, int y, out string updatedJson, out bool isHit)
    {
        updatedJson = battleMapJson;
        isHit = false;
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return false;
        }

        if (y < 0 || y >= sectors.Count)
        {
            return false;
        }

        if (sectors[y] is not JsonArray row || x < 0 || x >= row.Count)
        {
            return false;
        }

        if (row[x] is not JsonObject sector)
        {
            return false;
        }

        var destroyed = sector["destroyed"]?.GetValue<bool>() ?? false;
        if (destroyed)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        isHit = entityType is not null and not "empty";

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

        if (y < 0 || y >= sectors.Count)
        {
            return false;
        }

        if (sectors[y] is not JsonArray row || x < 0 || x >= row.Count)
        {
            return false;
        }

        if (row[x] is not JsonObject sector)
        {
            return false;
        }

        var entityType = sector["entity"]?["type"]?.GetValue<string>();
        if (entityType is not "empty")
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
}
