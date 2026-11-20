
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

public class HomeController : Controller
{
    [Route("Home/Error")]
    public IActionResult Error() => View();
}