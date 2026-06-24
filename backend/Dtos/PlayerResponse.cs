namespace backend.Dtos;

public class PlayerResponse
{
    public string Name { get; set; } = null!;
    public int Wins { get; set; }
    public int Loses { get; set; }
    public int TotalGames { get; set; }
}
