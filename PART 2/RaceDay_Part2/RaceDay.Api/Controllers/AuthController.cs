using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(RaceDayContext db, IConfiguration configuration) : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();

    ///Register a Participant or an Organiser. Organiser registration requires the configured organiser code.
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = request.Role?.Trim();
        if (role is not (UserRoles.Organiser or UserRoles.Participant))
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        if (role == UserRoles.Organiser &&
            (string.IsNullOrWhiteSpace(request.OrganiserCode) ||
             request.OrganiserCode != configuration["OrganiserRegistrationCode"]))
            return Forbid();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Created("/api/profile", ToResponse(user));
    }

    ///Log in and establish an authenticated cookie session.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });

        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Role", user.Role);
        return Ok(new { message = "Login successful.", user = ToResponse(user) });
    }

    ///Log out and clear the authentication cookie and session.
    [HttpPost("logout"), Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }

    private static UserResponse ToResponse(User u) => new(u.UserId, u.FirstName, u.LastName, u.Email, u.Role, u.CreatedAt);
}
