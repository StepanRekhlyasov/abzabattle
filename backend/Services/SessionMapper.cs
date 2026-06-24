using System.Text.Json;
using System.Text.Json.Nodes;
using backend.Dtos;
using backend.Models;

namespace backend.Services;

public static class SessionMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static SessionResponse ToResponse(
        GameSession session,
        string? viewerPlayerName = null,
        IReadOnlyDictionary<string, User>? users = null)
    {
        var rebelOccupied = !string.IsNullOrWhiteSpace(session.RebelPlayerName);
        var imperialOccupied = !string.IsNullOrWhiteSpace(session.ImperialPlayerName);
        var viewerIsParticipant = viewerPlayerName is not null &&
            (viewerPlayerName == session.RebelPlayerName ||
             viewerPlayerName == session.ImperialPlayerName);
        var viewerIsRebel = viewerPlayerName == session.RebelPlayerName;
        var viewerIsImperial = viewerPlayerName == session.ImperialPlayerName;

        return new SessionResponse
        {
            Id = session.Id,
            Name = session.Name,
            PtsLimit = session.PtsLimit,
            MapSize = session.MapSize,
            Rebel = new SessionSideResponse
            {
                Player = rebelOccupied
                    ? ToPlayer(session.RebelPlayerName!, users)
                    : null,
                BattleMap = MapBattleMap(
                    session.RebelBattleMapJson,
                    viewerIsRebel ? false : viewerIsImperial ? true : null),
            },
            Imperial = new SessionSideResponse
            {
                Player = imperialOccupied
                    ? ToPlayer(session.ImperialPlayerName!, users)
                    : null,
                BattleMap = MapBattleMap(
                    session.ImperialBattleMapJson,
                    viewerIsImperial ? false : viewerIsRebel ? true : null),
            },
            CurrentTurn = session.CurrentTurn,
            Status = session.Status,
            CreatorPlayerName = session.CreatorPlayerName,
            WinnerPlayerName = session.WinnerPlayerName,
            CanJoin = session.Status == SessionStatus.Pending &&
                      (!rebelOccupied || !imperialOccupied) &&
                      !viewerIsParticipant,
        };
    }

    public static PlayerResponse ToPlayer(string playerName, IReadOnlyDictionary<string, User>? users = null)
    {
        User? user = null;
        users?.TryGetValue(playerName, out user);

        return new PlayerResponse
        {
            Name = playerName,
            Wins = user?.Wins ?? 0,
            Loses = user?.Loses ?? 0,
            TotalGames = user?.TotalGames ?? 0,
        };
    }

    public static UserResponse ToUserResponse(User user) => new()
    {
        Name = user.Name,
        Wins = user.Wins,
        Loses = user.Loses,
        TotalGames = user.TotalGames,
    };

    private static JsonElement? MapBattleMap(string? battleMapJson, bool? hideUnits)
    {
        if (string.IsNullOrWhiteSpace(battleMapJson))
        {
            return null;
        }
        if (hideUnits is null)
        {
            return JsonSerializer.Deserialize<JsonElement>(battleMapJson, JsonOptions);
        }
        var root = JsonNode.Parse(battleMapJson);
        if (root?["sectors"] is not JsonArray sectors)
        {
            return JsonSerializer.Deserialize<JsonElement>(battleMapJson, JsonOptions);
        }
        foreach (var row in sectors)
        {
            if (row is not JsonArray rowSectors)
            {
                continue;
            }
            foreach (var sector in rowSectors)
            {
                if (sector is not JsonObject sectorObject)
                {
                    continue;
                }
                ApplyVisibility(sectorObject, hideUnits.Value);
            }
        }
        return JsonSerializer.SerializeToElement(root, JsonOptions);
    }

    private static void ApplyVisibility(JsonObject sectorObject, bool hideUnits)
    {
        var entityType = sectorObject["entity"]?["type"]?.GetValue<string>();
        var destroyed = sectorObject["destroyed"]?.GetValue<bool>() ?? false;
        if (hideUnits)
        {
            if (entityType is not null and not "empty")
            {
                sectorObject["hidden"] = !destroyed;
            }
            else if (destroyed)
            {
                sectorObject["hidden"] = false;
            }
            return;
        }
        if (entityType is not null and not "empty")
        {
            sectorObject["hidden"] = false;
        }
    }
}
