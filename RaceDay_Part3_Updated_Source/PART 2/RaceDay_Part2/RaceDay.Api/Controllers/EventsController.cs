using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Public event catalogue. Includes categories and route details needed by the MVC pages.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? eventType)
    {
        var query = db.Events.AsNoTracking()
            .Include(e => e.Categories)
            .Include(e => e.Routes)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.EventName.Contains(search) || e.Venue.Contains(search) || e.Province.Contains(search));
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);
        var events = await query.OrderBy(e => e.EventDate).Select(e => new
        {
            e.EventId, e.OrganiserId, e.EventName, e.Description, e.EventDate, e.Venue,
            e.Province, e.Status, e.EventType, e.BannerImageUrl,
            Categories = e.Categories.Select(c => new { c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm, c.MaxParticipants, c.EntryFee }),
            Routes = e.Routes.Select(r => new { r.RouteId, r.EventId, r.RouteName, r.DistanceKm, r.StartLocation, r.FinishLocation, r.RouteDescription, r.MapUrl })
        }).ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await db.Events.AsNoTracking().Where(e => e.EventId == id).Select(e => new
        {
            e.EventId, e.OrganiserId, e.EventName, e.Description, e.EventDate, e.Venue,
            e.Province, e.Status, e.EventType, e.BannerImageUrl,
            Categories = e.Categories.Select(c => new { c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm, c.MaxParticipants, c.EntryFee }),
            Routes = e.Routes.Select(r => new { r.RouteId, r.EventId, r.RouteName, r.DistanceKm, r.StartLocation, r.FinishLocation, r.RouteDescription, r.MapUrl })
        }).FirstOrDefaultAsync();
        return ev is null ? NotFound(new { message = "Event not found." }) : Ok(ev);
    }

    [HttpGet("mine"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Mine()
    {
        var events = await db.Events.AsNoTracking()
            .Where(e => e.OrganiserId == CurrentUserId)
            .Include(e => e.Categories)
            .Include(e => e.Routes)
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventId, e.EventName, e.Description, e.EventDate, e.Venue, e.Province,
                e.Status, e.EventType, e.BannerImageUrl,
                Categories = e.Categories.Select(c => new { c.CategoryId, c.CategoryName, c.DistanceKm, c.MaxParticipants, c.EntryFee }),
                EnrolmentCount = e.Categories.SelectMany(c => c.Enrollments).Count()
            }).ToListAsync();
        return Ok(events);
    }

    [HttpPost, Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Create(EventRequest request)
    {
        var ev = new Event
        {
            OrganiserId = CurrentUserId,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim() ?? "",
            EventDate = request.EventDate,
            Venue = request.Venue.Trim(),
            Province = request.Province.Trim(),
            EventType = request.EventType.Trim(),
            Status = "Scheduled"
        };
        ev.Routes.Add(new Route
        {
            RouteName = $"{ev.EventName} route",
            DistanceKm = request.DistanceKm,
            StartLocation = request.StartLocation?.Trim() ?? ev.Venue,
            FinishLocation = request.FinishLocation?.Trim() ?? ev.Venue,
            RouteDescription = request.RouteDescription?.Trim() ?? "",
            MapUrl = request.MapUrl
        });
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId, ev.OrganiserId, ev.EventName, ev.Description, ev.EventDate, ev.Venue, ev.Province, ev.Status, ev.EventType, ev.BannerImageUrl, Routes = ev.Routes.Select(r => new { r.RouteId, r.DistanceKm, r.StartLocation, r.FinishLocation, r.RouteDescription, r.MapUrl }) });
    }

    [HttpPut("{id:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var ev = await db.Events.Include(e => e.Routes).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        ev.EventName = request.EventName.Trim();
        ev.Description = request.Description?.Trim() ?? "";
        ev.EventDate = request.EventDate;
        ev.Venue = request.Venue.Trim();
        ev.Province = request.Province.Trim();
        ev.EventType = request.EventType.Trim();
        var route = ev.Routes.FirstOrDefault();
        if (route is null)
        {
            route = new Route { RouteName = $"{ev.EventName} route" };
            ev.Routes.Add(route);
        }
        route.RouteName = $"{ev.EventName} route";
        route.DistanceKm = request.DistanceKm;
        route.StartLocation = request.StartLocation?.Trim() ?? ev.Venue;
        route.FinishLocation = request.FinishLocation?.Trim() ?? ev.Venue;
        route.RouteDescription = request.RouteDescription?.Trim() ?? "";
        route.MapUrl = request.MapUrl;
        await db.SaveChangesAsync();
        return Ok(new { ev.EventId, ev.OrganiserId, ev.EventName, ev.Description, ev.EventDate, ev.Venue, ev.Province, ev.Status, ev.EventType, ev.BannerImageUrl, Routes = ev.Routes.Select(r => new { r.RouteId, r.DistanceKm, r.StartLocation, r.FinishLocation, r.RouteDescription, r.MapUrl }) });
    }

    [HttpDelete("{id:int}"), Authorize(Roles = UserRoles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await db.Events.Include(e => e.Categories).ThenInclude(c => c.Enrollments)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbid();
        if (ev.Categories.Any(c => c.Enrollments.Count > 0))
            return Conflict(new { message = "This event has enrolments and cannot be deleted." });
        db.Events.Remove(ev);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
