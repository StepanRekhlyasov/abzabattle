using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class LoginController(AppDbContext db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { detail = "Name is required" });
        }

        var name = request.Name.Trim();
        var exists = await db.Users.AnyAsync(u => u.Name == name);

        if (!exists)
        {
            db.Users.Add(new User { Name = name, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        return Ok(new LoginResponse { Success = true });
    }
}
