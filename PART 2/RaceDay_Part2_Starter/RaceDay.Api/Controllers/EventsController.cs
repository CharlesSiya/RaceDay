using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/events")]
public class EventsController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool ValidType(string type) => new[] { "Run", "Walk", "Cycle" }.Contains(type, StringComparer.OrdinalIgnoreCase);

    /// <summary>List events. Public read access.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await db.Events
        .Include(e => e.Routes).Include(e => e.Categories)
        .OrderBy(e => e.EventDate).ToListAsync());

    /// <summary>Get one event and its routes and categories.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await db.Events.Include(e => e.Routes).Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventId == id);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Create an event and its route. Organiser only.</summary>
    [HttpPost, Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (!ValidType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });
        var ev = new Event
        {
            OrganiserId = CurrentUserId, EventName = request.EventName.Trim(), Description = request.Description,
            EventDate = request.EventDate, Venue = request.Venue.Trim(), Province = request.Province.Trim(),
            EventType = request.EventType.Trim(), Status = "Scheduled", CreatedAt = DateTime.UtcNow
        };
        ev.Routes.Add(new Route
        {
            RouteName = ev.EventName + " Route", DistanceKm = request.DistanceKm,
            StartLocation = request.StartLocation ?? ev.Venue, FinishLocation = request.FinishLocation ?? ev.Venue,
            RouteDescription = request.RouteDescription ?? "", MapUrl = request.MapUrl
        });
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.EventId }, ev);
    }

    /// <summary>Update an event owned by the logged-in organiser.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        if (!ValidType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });
        var ev = await db.Events.Include(e => e.Routes).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        ev.EventName = request.EventName.Trim(); ev.Description = request.Description; ev.EventDate = request.EventDate;
        ev.Venue = request.Venue.Trim(); ev.Province = request.Province.Trim(); ev.EventType = request.EventType.Trim();
        var route = ev.Routes.FirstOrDefault();
        if (route is null) ev.Routes.Add(new Route { RouteName = ev.EventName + " Route", DistanceKm = request.DistanceKm });
        else { route.DistanceKm = request.DistanceKm; route.StartLocation = request.StartLocation ?? ev.Venue;
            route.FinishLocation = request.FinishLocation ?? ev.Venue; route.RouteDescription = request.RouteDescription ?? ""; route.MapUrl = request.MapUrl; }
        await db.SaveChangesAsync();
        return Ok(ev);
    }

    /// <summary>Delete an event owned by the logged-in organiser, if it has no enrolments.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await db.Events.Include(e => e.Categories).ThenInclude(c => c.Enrollments).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        if (ev.Categories.Any(c => c.Enrollments.Any()))
            return Conflict(new { message = "Cannot delete an event with enrolments." });
        db.Events.Remove(ev);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
