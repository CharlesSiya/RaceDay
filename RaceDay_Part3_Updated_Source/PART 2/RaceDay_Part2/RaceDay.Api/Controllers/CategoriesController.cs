using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/events/{eventId:int}/categories")]
public class CategoriesController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>List categories for an event.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(int eventId)
    {
        if (!await db.Events.AnyAsync(e => e.EventId == eventId)) return NotFound(new { message = "Event not found." });
        return Ok(await db.Categories.Where(c => c.EventId == eventId).ToListAsync());
    }

    /// <summary>Create a category for an event owned by the organiser.</summary>
    [HttpPost, Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        var category = new Category
        {
            EventId = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKm = request.DistanceKm,
            MaxParticipants = request.MaxParticipants,
            EntryFee = request.EntryFee
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/categories/{category.CategoryId}", category);
    }

    /// Update a category for an event owned by the organiser.</summary>
    [HttpPut("{categoryId:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Update(int eventId, int categoryId, CategoryRequest request)
    {
        var category = await db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.EventId == eventId);
        if (category is null) return NotFound();
        if (category.Event!.OrganiserId != CurrentUserId) return Forbid();
        category.CategoryName = request.CategoryName.Trim(); category.DistanceKm = request.DistanceKm;
        category.MaxParticipants = request.MaxParticipants; category.EntryFee = request.EntryFee;
        await db.SaveChangesAsync();
        return Ok(category);
    }

    /// <summary>Delete a category if no participants have enrolled in it.</summary>
    [HttpDelete("{categoryId:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Delete(int eventId, int categoryId)
    {
        var category = await db.Categories.Include(c => c.Event).Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.EventId == eventId);
        if (category is null) return NotFound();
        if (category.Event!.OrganiserId != CurrentUserId) return Forbid();
        if (category.Enrollments.Count > 0) return Conflict(new { message = "Cannot delete a category with enrolments." });
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
