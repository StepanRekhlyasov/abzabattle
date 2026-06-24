using System.Text.Json;

namespace backend.Dtos;

public class SessionSideResponse
{
    public PlayerResponse? Player { get; set; }
    public JsonElement? BattleMap { get; set; }
}
