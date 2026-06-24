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
    [HttpGet("users/online")]
    public ActionResult<IEnumerable<UserResponse>> GetOnlineUsers()
    {
        return Ok(presence.GetOnlineUsers().Select(name => new UserResponse { Name = name }));
    }

    [HttpGet("user")]
    public async Task<ActionResult<UserResponse>> GetByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { detail = "Name is required" });
        }

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name == name.Trim());

        if (user is null)
        {
            return NotFound(new { detail = "User not found" });
        }

        return Ok(new UserResponse
        {
            Name = user.Name,
            Wins = user.Wins,
            Loses = user.Loses,
            TotalGames = user.TotalGames,
        });
    }
}
