using System.Text.Json;

namespace backend.Services;

public static class BattleMapValidator
{
    public static bool HasRemainingUnits(string battleMapJson)
    {
        using var document = JsonDocument.Parse(battleMapJson);
        if (!document.RootElement.TryGetProperty("sectors", out var sectors) ||
            sectors.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var row in sectors.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var sector in row.EnumerateArray())
            {
                if (!sector.TryGetProperty("entity", out var entity) ||
                    !entity.TryGetProperty("type", out var type))
                {
                    continue;
                }

                if (type.GetString() is not { } entityType || entityType == "empty")
                {
                    continue;
                }

                var destroyed = sector.TryGetProperty("destroyed", out var destroyedProperty) &&
                                destroyedProperty.GetBoolean();
                if (!destroyed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static int CalculatePtsSpent(string battleMapJson)
    {
        var ptsByType = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["star-destroyer"] = 40,
            ["mon-calamari"] = 30,
            ["tie-fighter"] = 10,
            ["nebulon-frigate"] = 20,
            ["x-wing"] = 15,
            ["u-wing"] = 20,
        };

        var spent = 0;
        using var document = JsonDocument.Parse(battleMapJson);
        if (!document.RootElement.TryGetProperty("sectors", out var sectors) ||
            sectors.ValueKind != JsonValueKind.Array)
        {
            return spent;
        }

        var countedIds = new HashSet<string>();

        foreach (var row in sectors.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var sector in row.EnumerateArray())
            {
                if (!sector.TryGetProperty("entity", out var entity) ||
                    !entity.TryGetProperty("type", out var typeProperty))
                {
                    continue;
                }

                var type = typeProperty.GetString();
                if (type is null or "empty" || !ptsByType.TryGetValue(type, out var cost))
                {
                    continue;
                }

                var id = entity.TryGetProperty("id", out var idProperty)
                    ? idProperty.GetString()
                    : null;

                if (id is not null)
                {
                    if (!countedIds.Add(id))
                    {
                        continue;
                    }
                }

                spent += cost;
            }
        }

        return spent;
    }
}
