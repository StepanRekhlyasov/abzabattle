using System.Text.Json;

namespace backend.Dtos;

public class CreateSessionRequest
{
    public string Name { get; set; } = null!;
    public string Faction { get; set; } = null!;
    public int PtsLimit { get; set; }
    public int MapSize { get; set; }
    public string PlayerName { get; set; } = null!;
    public JsonElement BattleMap { get; set; }
}
