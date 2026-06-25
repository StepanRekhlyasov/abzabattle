using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class SessionBroadcastService(
    IHubContext<SessionHub> hub,
    SessionConnectionService connections,
    SessionUserService sessionUsers)
{
    public async Task BroadcastUpdatedAsync(GameSession session)
    {
        var users = await sessionUsers.GetUsersForSessionAsync(session);

        foreach (var (connectionId, name) in connections.GetConnections())
        {
            var response = SessionMapper.ToResponse(session, name, users);
            await hub.Clients.Client(connectionId).SendAsync("SessionUpdated", response);
        }
    }

    public async Task BroadcastDeletedAsync(Guid sessionId)
    {
        foreach (var (connectionId, _) in connections.GetConnections())
        {
            await hub.Clients.Client(connectionId).SendAsync("SessionDeleted", sessionId);
        }
    }
}
