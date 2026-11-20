using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class EventsController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? search, string? eventType)
    {
        var query = "api/events";
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) parameters.Add("search=" + Uri.EscapeDataString(search));
        if (!string.IsNullOrWhiteSpace(eventType)) parameters.Add("eventType=" + Uri.EscapeDataString(eventType));
        if (parameters.Count > 0) query += "?" + string.Join("&", parameters);
        ViewBag.Search = search;
        ViewBag.EventType = eventType;
        return View(await api.GetJsonAsync(query, HttpContext));
    }

    public async Task<IActionResult> Details(int id)
    {
        var data = await api.GetJsonAsync($"api/events/{id}", HttpContext);
        return data is null ? NotFound() : View(data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int categoryId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant") return RedirectToAction("Login", "Account");
        var (response, _) = await api.SendAsync(HttpMethod.Post, "api/enrollments", HttpContext, new { categoryId });
        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] = response.IsSuccessStatusCode
            ? "Your enrolment was submitted successfully." : "Enrolment could not be completed. You may already be enrolled or the category may be full.";
        return RedirectToAction(nameof(Index));
    }
}
