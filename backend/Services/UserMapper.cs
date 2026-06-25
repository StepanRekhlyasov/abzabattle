using backend.Dtos;
using backend.Models;

namespace backend.Services;

public static class UserMapper
{
    public static UserResponse ToResponse(User user, bool isOnline) => new()
    {
        Name = user.Name,
        Wins = user.Wins,
        Loses = user.Loses,
        TotalGames = user.TotalGames,
        Status = isOnline ? "online" : "offline",
    };
}
