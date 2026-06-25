using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.Hubs;

public class SessionHub(
    AppDbContext db,
    SessionConnectionService connections,
    SessionUserService sessionUsers) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var name = Context.GetHttpContext()?.Request.Query["name"].ToString();
        if (string.IsNullOrWhiteSpace(name))
        {
            Context.Abort();
            return;
        }
        var viewer = name.Trim();
        connections.Connect(Context.ConnectionId, viewer);
        var sessions = await db.Sessions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        var users = await sessionUsers.GetUsersForSessionsAsync(sessions);
        var snapshot = sessions.Select(s => SessionMapper.ToResponse(s, viewer, users)).ToList();
        await Clients.Caller.SendAsync("SessionSnapshot", snapshot);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connections.Disconnect(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
