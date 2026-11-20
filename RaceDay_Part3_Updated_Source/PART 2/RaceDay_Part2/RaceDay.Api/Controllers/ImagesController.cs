using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ImagesController(RaceDayContext db, IBlobStorageService storage) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("events/{eventId:int}/banner"), Authorize(Roles = UserRoles.Organiser)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Select an image file." });
        var ev = await db.Events.FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        try
        {
            ev.BannerImageUrl = await storage.UploadAsync(file, "event-banners", cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(new { imageUrl = ev.BannerImageUrl });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
    }

    [HttpPost("profile/photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Select an image file." });
        var user = await db.Users.FindAsync(new object[] { CurrentUserId }, cancellationToken);
        if (user is null) return NotFound();
        try
        {
            user.ProfileImageUrl = await storage.UploadAsync(file, "profile-photos", cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(new { imageUrl = user.ProfileImageUrl });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
    }
}
