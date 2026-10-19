using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class ResultsController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Capture a result for an enrolment in an event owned by this Organiser.</summary>
    [HttpPost("enrollments/{enrollmentId:int}/result"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Capture(int enrollmentId, ResultRequest request)
    {
        if (request.FinishTime <= TimeSpan.Zero) return BadRequest(new { message = "FinishTime must be positive (hh:mm:ss)." });
        var enrolment = await db.Enrollments.Include(e => e.Category).ThenInclude(c => c!.Event)
            .Include(e => e.Result).FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        if (enrolment is null) return NotFound();
        if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();
        if (enrolment.Result is not null) return Conflict(new { message = "A result already exists for this enrolment." });
        var result = new Result
        {
            EnrollmentId = enrollmentId,
            Position = request.Position,
            FinishTime = request.FinishTime,
            PaceMinutesPerKm = request.PaceMinutesPerKm,
            ResultDate = DateTime.UtcNow
        };
        db.Results.Add(result);
        await db.SaveChangesAsync();
        return Created($"/api/results/{result.ResultId}", result);
    }

    /// <summary>List results for the authenticated Participant only.</summary>
    [HttpGet("results/mine"), Authorize(Roles = UserRoles.Participant)]
    public async Task<IActionResult> MyResults()
    {
        var results = await db.Results.Include(r => r.Enrollment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .Where(r => r.Enrollment!.ParticipantId == CurrentUserId)
            .Select(r => new {
                r.ResultId,
                r.FinishTime,
                r.Position,
                r.PaceMinutesPerKm,
                r.ResultDate,
                EventName = r.Enrollment!.Category!.Event!.EventName,
                CategoryName = r.Enrollment.Category.CategoryName
            })
            .ToListAsync();
        return Ok(results);
    }
}
