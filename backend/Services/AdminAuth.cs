namespace backend.Services;

public static class AdminAuth
{
    public const string AdminUserName = "Admin";

    public static bool IsAdmin(string? name) =>
        string.Equals(name?.Trim(), AdminUserName, StringComparison.Ordinal);
}
