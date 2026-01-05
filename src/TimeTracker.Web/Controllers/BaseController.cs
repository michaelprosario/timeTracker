using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Web.Controllers;

public class BaseController : Controller
{
    protected Guid GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Guid.Empty;
        }
        
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }
    
    protected string GetCurrentUserName()
    {
        return HttpContext.Session.GetString("UserName") ?? "Guest";
    }
}
