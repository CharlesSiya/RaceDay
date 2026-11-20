using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ProfileController(RaceDayApiClient api) : Controller
{
    private bool SignedIn() =>
        !string.IsNullOrEmpty(
            HttpContext.Session.GetString("UserId"));

    public async Task<IActionResult> Index()
    {
        if (!SignedIn())
            return RedirectToAction("Login", "Account");

        return View(await api.GetJsonAsync(
            "api/profile", HttpContext));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        string firstName,
        string lastName,
        string email)
    {
        if (!SignedIn())
            return RedirectToAction("Login", "Account");

        var (response, _) = await api.SendAsync(
            HttpMethod.Put,
            "api/profile",
            HttpContext,
            new { firstName, lastName, email });

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Profile updated."
                : "Profile could not be updated. Check the details and email address.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(
        IFormFile profilePicture)
    {
        if (!SignedIn())
            return RedirectToAction("Login", "Account");

        if (profilePicture is null || profilePicture.Length == 0)
        {
            TempData["Error"] = "Choose an image first.";
            return RedirectToAction(nameof(Index));
        }

        var (response, _) = await api.SendFileAsync(
            "api/profile/photo", HttpContext, profilePicture);

        TempData[response.IsSuccessStatusCode ? "Message" : "Error"] =
            response.IsSuccessStatusCode
                ? "Profile picture uploaded."
                : "Profile picture upload failed. Check Azure configuration and image type/size.";

        return RedirectToAction(nameof(Index));
    }
}