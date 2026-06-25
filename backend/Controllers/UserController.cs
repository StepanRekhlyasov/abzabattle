using backend.Data;
using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class UserController(AppDbContext db, PresenceService presence) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var onlineUsers = new HashSet<string>(presence.GetOnlineUsers(), StringComparer.Ordinal);
        var users = await db.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return Ok(users.Select(user => UserMapper.ToResponse(user, onlineUsers.Contains(user.Name))));
    }

    [HttpGet("users/online")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetOnlineUsers()
    {
        var onlineNames = new HashSet<string>(presence.GetOnlineUsers(), StringComparer.Ordinal);
        var users = await db.Users
            .AsNoTracking()
            .Where(u => onlineNames.Contains(u.Name))
            .OrderBy(u => u.Name)
            .ToListAsync();

        return Ok(users.Select(user => UserMapper.ToResponse(user, isOnline: true)));
    }

    [HttpGet("user")]
    public async Task<ActionResult<UserResponse>> GetByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { detail = "Name is required" });
        }

        var trimmedName = name.Trim();
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name == trimmedName);

        if (user is null)
        {
            return NotFound(new { detail = "User not found" });
        }

        var isOnline = presence.GetOnlineUsers().Contains(trimmedName, StringComparer.Ordinal);
        return Ok(UserMapper.ToResponse(user, isOnline));
    }
}
