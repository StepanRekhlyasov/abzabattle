using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services;

public class SessionBroadcastService(IHubContext<SessionHub> hub, SessionConnectionService connections)
{
    public async Task BroadcastUpdatedAsync(GameSession session)
    {
        foreach (var (connectionId, name) in connections.GetConnections())
        {
            var response = SessionMapper.ToResponse(session, name);
            await hub.Clients.Client(connectionId).SendAsync("SessionUpdated", response);
        }
    }
}
