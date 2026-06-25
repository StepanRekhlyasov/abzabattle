using backend.Data;
using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.Hubs;

public class PresenceHub(PresenceService presence, AppDbContext db) : Hub
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

        var snapshot = await BuildUserSnapshotAsync();
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

    private async Task<List<UserResponse>> BuildUserSnapshotAsync()
    {
        var onlineUsers = new HashSet<string>(presence.GetOnlineUsers(), StringComparer.Ordinal);
        var users = await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users
            .Select(user => UserMapper.ToResponse(user, onlineUsers.Contains(user.Name)))
            .ToList();
    }
}
