using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        var userId = GetCurrentUserId();
        if (userId != Guid.Empty)
        {
            return RedirectToAction("Index", "TimeSheet");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
