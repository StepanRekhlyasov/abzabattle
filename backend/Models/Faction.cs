namespace backend.Models;

public static class Faction
{
    public const string Imperial = "imperial";
    public const string Rebel = "rebel";

    public static string Opposite(string faction) =>
        faction == Imperial ? Rebel : Imperial;
}
