using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/profile"), Authorize]
public class ProfileController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get the authenticated user's own profile.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var u = await db.Users.FindAsync(CurrentUserId);
        return u is null ? NotFound() : Ok(new UserResponse(u.UserId, u.FirstName, u.LastName, u.Email, u.Role, u.CreatedAt));
    }

    /// <summary>Update the authenticated user's own profile details.</summary>
    [HttpPut]
    public async Task<IActionResult> Update(ProfileUpdateRequest request)
    {
        var u = await db.Users.FindAsync(CurrentUserId);
        if (u is null) return NotFound();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.UserId != u.UserId))
            return Conflict(new { message = "Email is already in use." });
        u.FirstName = request.FirstName.Trim();
        u.LastName = request.LastName.Trim();
        u.Email = email;
        await db.SaveChangesAsync();
        return Ok(new UserResponse(u.UserId, u.FirstName, u.LastName, u.Email, u.Role, u.CreatedAt));
    }
}
