namespace backend.Dtos;

public class SessionActionLogResponse
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string PlayerName { get; set; } = null!;
    public string ActionKind { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string PayloadJson { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
