namespace backend.Dtos;

public class SessionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int PtsLimit { get; set; }
    public int MapSize { get; set; }
    public SessionSideResponse Rebel { get; set; } = null!;
    public SessionSideResponse Imperial { get; set; } = null!;
    public string CurrentTurn { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool CanJoin { get; set; }
    public string? JoinBlockedReason { get; set; }
    public string CreatorPlayerName { get; set; } = null!;
    public string? WinnerPlayerName { get; set; }
    public int HitsThisTurn { get; set; }
    public DateTime CreatedAt { get; set; }
}
