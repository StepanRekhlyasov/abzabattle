using System.Text.Json;

namespace backend.Dtos;

public class StartBattleRequest
{
    public string PlayerName { get; set; } = null!;
    public JsonElement BattleMap { get; set; }
}
