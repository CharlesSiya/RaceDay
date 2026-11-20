using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class EnrollmentsController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Enrol the authenticated Participant in a category.</summary>
    [HttpPost("enrollments"), Authorize(Roles = UserRoles.Participant)]
    public async Task<IActionResult> Enrol(EnrollmentRequest request)
    {
        var category = await db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);
        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event!.EventDate < DateTime.UtcNow) return BadRequest(new { message = "This event has already taken place." });
        var existing = await db.Enrollments.CountAsync(e => e.CategoryId == category.CategoryId);
        if (existing >= category.MaxParticipants) return Conflict(new { message = "Category is full." });
        if (await db.Enrollments.AnyAsync(e => e.ParticipantId == CurrentUserId && e.CategoryId == category.CategoryId))
            return Conflict(new { message = "You are already enrolled in this category." });

        var enrollment = new Enrollment
        {
            ParticipantId = CurrentUserId,
            CategoryId = category.CategoryId,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Enrolled"
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();
        return Created($"/api/enrollments/{enrollment.EnrollmentId}", enrollment);
    }

    /// <summary>View all enrolments for events owned by the authenticated Organiser.</summary>
    [HttpGet("organiser/enrollments"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> OrganiserEnrollments()
    {
        var items = await db.Enrollments.Include(e => e.Participant).Include(e => e.Category).ThenInclude(c => c!.Event)
            .Where(e => e.Category!.Event!.OrganiserId == CurrentUserId)
            .Select(e => new {
                e.EnrollmentId,
                e.EnrollmentDate,
                e.Status,
                e.BibNumber,
                Participant = e.Participant!.FirstName + " " + e.Participant.LastName,
                e.Participant.Email,
                e.Category!.CategoryName,
                EventName = e.Category.Event!.EventName
            })
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>View the authenticated Participant's own enrolments.</summary>
    [HttpGet("enrollments/mine"), Authorize(Roles = UserRoles.Participant)]
    public async Task<IActionResult> Mine() => Ok(await db.Enrollments.Include(e => e.Category).ThenInclude(c => c!.Event)
        .Where(e => e.ParticipantId == CurrentUserId)
        .Select(e => new {
            e.EnrollmentId,
            e.EnrollmentDate,
            e.Status,
            e.BibNumber,
            e.Category!.CategoryName,
            EventName = e.Category.Event!.EventName,
            e.Category.DistanceKm
        })
        .ToListAsync());
}
