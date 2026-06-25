namespace backend.Models;

public class GameSession
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Status { get; set; } = SessionStatus.Pending;
    public string CurrentTurn { get; set; } = Faction.Rebel;
    public int PtsLimit { get; set; }
    public int MapSize { get; set; }
    public string CreatorPlayerName { get; set; } = null!;
    public string? RebelPlayerName { get; set; }
    public string? ImperialPlayerName { get; set; }
    public string? RebelBattleMapJson { get; set; }
    public string? ImperialBattleMapJson { get; set; }
    public string? WinnerPlayerName { get; set; }
    public int HitsThisTurn { get; set; }
    public DateTime CreatedAt { get; set; }
}
