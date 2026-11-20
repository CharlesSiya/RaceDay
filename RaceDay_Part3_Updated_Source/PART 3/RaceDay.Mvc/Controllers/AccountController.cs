
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;

public class AccountController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var (response, body) = await api.SendAsync(
            HttpMethod.Post, "api/auth/login", HttpContext,
            new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Login failed. Check your email and password.");
            return View();
        }

        using var json = JsonDocument.Parse(body);
        var user = json.RootElement.GetProperty("user");

        HttpContext.Session.SetString("UserId", user.GetProperty("userId").ToString());
        HttpContext.Session.SetString("Role", user.GetProperty("role").GetString() ?? "");
        HttpContext.Session.SetString("FirstName", user.GetProperty("firstName").GetString() ?? "");

        return RedirectToAction("Index", "Events");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string firstName, string lastName, string email,
        string password, string role, string? organiserCode)
    {
        var (response, _) = await api.SendAsync(
            HttpMethod.Post, "api/auth/register", HttpContext,
            new { firstName, lastName, email, password, role, organiserCode });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Registration failed. Check your details and, for Organiser accounts, the registration code.");
            return View();
        }

        TempData["Message"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await api.SendAsync(HttpMethod.Post, "api/auth/logout", HttpContext);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}