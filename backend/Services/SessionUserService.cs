using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SessionUserService(AppDbContext db)
{
    public async Task<IReadOnlyDictionary<string, User>> GetUsersForSessionAsync(GameSession session)
    {
        var names = GetPlayerNames(session).ToList();
        return await LoadUsersAsync(names);
    }

    public async Task<IReadOnlyDictionary<string, User>> GetUsersForSessionsAsync(IEnumerable<GameSession> sessions)
    {
        var names = sessions
            .SelectMany(GetPlayerNames)
            .Distinct()
            .ToList();

        return await LoadUsersAsync(names);
    }

    private static IEnumerable<string> GetPlayerNames(GameSession session)
    {
        if (!string.IsNullOrWhiteSpace(session.RebelPlayerName))
        {
            yield return session.RebelPlayerName;
        }

        if (!string.IsNullOrWhiteSpace(session.ImperialPlayerName))
        {
            yield return session.ImperialPlayerName;
        }
    }

    private async Task<IReadOnlyDictionary<string, User>> LoadUsersAsync(IReadOnlyCollection<string> names)
    {
        if (names.Count == 0)
        {
            return new Dictionary<string, User>();
        }

        return await db.Users
            .AsNoTracking()
            .Where(user => names.Contains(user.Name))
            .ToDictionaryAsync(user => user.Name);
    }
}
