using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController(RaceDayApiClient api) : Controller
{
    private bool Allowed() =>
        HttpContext.Session.GetString("Role") == "Participant";

    public async Task<IActionResult> MyEvents()
    {
        if (!Allowed())
            return RedirectToAction("Login", "Account");

        return View(await api.GetJsonAsync(
            "api/enrollments/mine", HttpContext));
    }

    public async Task<IActionResult> MyResults()
    {
        if (!Allowed())
            return RedirectToAction("Login", "Account");

        return View(await api.GetJsonAsync(
            "api/results/mine", HttpContext));
    }
}