namespace backend.Models;

public class User
{
    public string Name { get; set; } = null!;
    public int Wins { get; set; }
    public int Loses { get; set; }
    public int TotalGames { get; set; }
    public DateTime CreatedAt { get; set; }
}
