namespace backend.Dtos;

public class UserResponse
{
    public required string Name { get; set; }
    public int Wins { get; set; }
    public int Loses { get; set; }
    public int TotalGames { get; set; }
    public string Status { get; set; } = "offline";
}
