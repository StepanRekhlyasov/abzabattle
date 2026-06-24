namespace backend.Dtos;

public class UseAbilityRequest
{
    public string PlayerName { get; set; } = null!;
    public string AbilityKind { get; set; } = null!;
    public int X { get; set; }
    public int Y { get; set; }
}
