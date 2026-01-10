using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Queries;
using TimeTracker.Core.Services;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class ProjectController : BaseController
{
    private readonly ProjectService _projectService;
    
    public ProjectController(ProjectService projectService)
    {
        _projectService = projectService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var query = new GetProjectsQuery { ActiveOnly = false };
        var result = await _projectService.GetProjectsAsync(query);
        
        var viewModel = new ProjectListViewModel
        {
            Projects = result.Data!.Select(p => new ProjectDetailsViewModel
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
        
        return View(viewModel);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        return View(new CreateProjectViewModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var command = new CreateProjectCommand
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
        
        var result = await _projectService.CreateProjectAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = result.Messages.Any() ? result.Messages.First() : "Project created successfully";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            return View(model);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(string code)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _projectService.GetProjectByCodeAsync(code);
        
        if (!result.Success)
        {
            TempData["Error"] = string.Join(", ", result.Errors);
            return RedirectToAction(nameof(Index));
        }
        
        var viewModel = new UpdateProjectViewModel
        {
            Code = result.Data!.Code,
            Name = result.Data.Name,
            Description = result.Data.Description,
            IsActive = result.Data.IsActive
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(UpdateProjectViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var command = new UpdateProjectCommand
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
        
        var result = await _projectService.UpdateProjectAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = result.Messages.Any() ? result.Messages.First() : "Project updated successfully";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            return View(model);
        }
    }
}
