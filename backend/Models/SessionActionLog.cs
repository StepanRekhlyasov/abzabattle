namespace backend.Models;

public class SessionActionLog
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int Sequence { get; set; }
    public string PlayerName { get; set; } = null!;
    public string ActionKind { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string PayloadJson { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
