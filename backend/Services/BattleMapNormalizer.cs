using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Services;

public static class BattleMapNormalizer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string PrepareForStorage(string battleMapJson)
    {
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return battleMapJson;
        }

        foreach (var row in sectors)
        {
            if (row is not JsonArray rowSectors)
            {
                continue;
            }

            foreach (var sector in rowSectors)
            {
                if (sector is JsonObject sectorObject)
                {
                    sectorObject["hidden"] = true;
                }
            }
        }

        return root.ToJsonString(JsonOptions);
    }
}
