using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Services;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserService _userService;
    
    public AccountController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var command = new RegisterUserCommand
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        
        var result = await _userService.RegisterUserAsync(command);
        
        if (!result.Success)
        {
            if (result.ValidationErrors.Any())
            {
                foreach (var error in result.ValidationErrors)
                {
                    foreach (var message in error.Value)
                    {
                        ModelState.AddModelError(error.Key, message);
                    }
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(model);
        }
        
        TempData["Success"] = "Registration successful! Please login.";
        return RedirectToAction(nameof(Login));
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var command = new LoginUserCommand
        {
            Email = model.Email,
            Password = model.Password
        };
        
        var result = await _userService.LoginAsync(command);
        
        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }
        
        HttpContext.Session.SetString("UserId", result.Data!.Id.ToString());
        HttpContext.Session.SetString("UserName", $"{result.Data.FirstName} {result.Data.LastName}");
        
        return RedirectToAction("Dashboard", "Home");
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
