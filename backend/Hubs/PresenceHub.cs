using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

public class PresenceHub(PresenceService presence) : Hub
{
    public const string Channel = "presence";

    public override async Task OnConnectedAsync()
    {
        var name = Context.GetHttpContext()?.Request.Query["name"].ToString();

        if (string.IsNullOrWhiteSpace(name))
        {
            Context.Abort();
            return;
        }

        name = name.Trim();
        var becameOnline = presence.Connect(Context.ConnectionId, name);

        await Groups.AddToGroupAsync(Context.ConnectionId, Channel);

        var snapshot = presence.GetOnlineUsers()
            .Select(userName => new UserResponse { Name = userName })
            .ToList();

        await Clients.Caller.SendAsync("PresenceSnapshot", snapshot);

        if (becameOnline)
        {
            await Clients.Group(Channel).SendAsync("PresenceUpdated", new PresenceUpdateDto
            {
                Name = name,
                Status = "online",
            });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var (name, wentOffline) = presence.Disconnect(Context.ConnectionId);

        if (name is not null && wentOffline)
        {
            await Clients.Group(Channel).SendAsync("PresenceUpdated", new PresenceUpdateDto
            {
                Name = name,
                Status = "offline",
            });
        }

        await base.OnDisconnectedAsync(exception);
    }
}
