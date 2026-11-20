using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController(RaceDayApiClient api) : Controller
{
    private bool Allowed() =>
        HttpContext.Session.GetString("Role") == "Organiser";

    private IActionResult LoginRedirect() =>
        RedirectToAction("Login", "Account");

    public async Task<IActionResult> Dashboard()
    {
        if (!Allowed()) return LoginRedirect();

        ViewBag.Events = await api.GetJsonAsync("api/events/mine", HttpContext);
        ViewBag.Enrolments = await api.GetJsonAsync(
            "api/organiser/enrollments", HttpContext);

        return View();
    }

    public async Task<IActionResult> Enrolments()
    {
        if (!Allowed()) return LoginRedirect();

        return View(await api.GetJsonAsync(
            "api/organiser/enrollments", HttpContext));
    }

    [HttpGet]
    public IActionResult CreateEvent() =>
        Allowed() ? View() : LoginRedirect();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(
        string eventName,
        string description,
        DateTime eventDate,
        string venue,
        string province,
        string eventType,
        decimal distanceKm,
        string? startLocation,
        string? finishLocation,
        string? routeDescription,
        string? mapUrl,
        IFormFile? bannerImage)
    {
        if (!Allowed()) return LoginRedirect();

        var body = new
        {
            eventName,
            description,
            eventDate,
            venue,
            province,
            eventType,
            distanceKm,
            startLocation,
            finishLocation,
            routeDescription,
            mapUrl
        };

        var (response, responseBody) = await api.SendAsync(
            HttpMethod.Post, "api/events", HttpContext, body);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "", "The API could not create this event. Check all required fields.");

            return View();
        }

        if (bannerImage is { Length: > 0 })
        {
            using var json = JsonDocument.Parse(responseBody);
            var id = json.RootElement.GetProperty("eventId").GetInt32();

            var (uploadResponse, _) = await api.SendFileAsync(
                $"api/events/{id}/banner", HttpContext, bannerImage);

            TempData[uploadResponse.IsSuccessStatusCode ? "Message" : "Error"] =
                uploadResponse.IsSuccessStatusCode
                    ? "Event created and banner uploaded."
                    : "Event created, but the banner could not be uploaded. Check Azure configuration.";
        }
        else
        {
            TempData["Message"] = "Event created successfully.";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (!Allowed()) return LoginRedirect();

        var data = await api.GetJsonAsync($"api/events/{id}", HttpContext);

        return data is null ? NotFound() : View(data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(
        int id,
        string eventName,
        string description,
        DateTime eventDate,
        string venue,
        string province,
        string eventType,
        decimal distanceKm,
        string? startLocation,
        string? finishLocation,
        string? routeDescription,
        string? mapUrl,
        IFormFile? bannerImage)
    {
        if (!Allowed()) return LoginRedirect();

        var body = new
        {
            eventName,
            description,
            eventDate,
            venue,
            province,
            eventType,
            distanceKm,
            startLocation,
            finishLocation,
            routeDescription,
            mapUrl
        };

        var (response, _) = await api.SendAsync(
            HttpMethod.Put, $"api/events/{id}", HttpContext, body);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "The event could not be updated.";
            return RedirectToAction(nameof(Dashboard));
        }

        if (bannerImage is { Length: > 0 })
        {
            var (uploadResponse, _) = await api.SendFileAsync(
                $"api/events/{id}/banner", HttpContext, bannerImage);

            TempData[uploadResponse.IsSuccessStatusCode ? "Message" : "Error"] =
                uploadResponse.IsSuccessStatusCode
                    ? "Event updated and banner uploaded."
                    : "Event updated, but banner upload failed.";
        }
        else
        {
            TempData["Message"] = "Event updated successfully.";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!Allowed()) return LoginRedirect();

        var (response, _) = await api.SendAsync(
            HttpMethod.Delete, $"api/events/{id}", HttpContext);

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Event deleted."
                : "Event could not be deleted. It may already have enrolments.";

        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Categories(int eventId)
    {
        if (!Allowed()) return LoginRedirect();

        ViewBag.EventId = eventId;
        ViewBag.Event = await api.GetJsonAsync(
            $"api/events/{eventId}", HttpContext);

        return View(await api.GetJsonAsync(
            $"api/events/{eventId}/categories", HttpContext));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(
        int eventId,
        string categoryName,
        decimal distanceKm,
        int maxParticipants,
        decimal entryFee)
    {
        if (!Allowed()) return LoginRedirect();

        var (response, _) = await api.SendAsync(
            HttpMethod.Post,
            $"api/events/{eventId}/categories",
            HttpContext,
            new { categoryName, distanceKm, maxParticipants, entryFee });

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Category added."
                : "Category could not be added.";

        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(
        int eventId, int categoryId)
    {
        if (!Allowed()) return LoginRedirect();

        var (response, _) = await api.SendAsync(
            HttpMethod.Delete,
            $"api/events/{eventId}/categories/{categoryId}",
            HttpContext);

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Category deleted."
                : "Category cannot be deleted if participants have enrolled.";

        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(
        int enrollmentId,
        int position,
        string finishTime,
        decimal paceMinutesPerKm)
    {
        if (!Allowed()) return LoginRedirect();

        if (!TimeSpan.TryParse(
            finishTime, CultureInfo.InvariantCulture, out var time)
            || time <= TimeSpan.Zero)
        {
            TempData["Error"] =
                "Enter a valid finish time in hh:mm:ss format.";

            return RedirectToAction(nameof(Enrolments));
        }

        var (response, _) = await api.SendAsync(
            HttpMethod.Post,
            $"api/enrollments/{enrollmentId}/result",
            HttpContext,
            new
            {
                position,
                finishTime = time.ToString("c", CultureInfo.InvariantCulture),
                paceMinutesPerKm
            });

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Race result saved."
                : "Result could not be saved.";

        return RedirectToAction(nameof(Enrolments));
    }
}