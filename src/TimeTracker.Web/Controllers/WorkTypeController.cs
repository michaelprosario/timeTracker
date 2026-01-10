using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Queries;
using TimeTracker.Core.Services;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class WorkTypeController : BaseController
{
    private readonly WorkTypeService _workTypeService;
    
    public WorkTypeController(WorkTypeService workTypeService)
    {
        _workTypeService = workTypeService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var query = new GetWorkTypesQuery { ActiveOnly = false };
        var result = await _workTypeService.GetWorkTypesAsync(query);
        
        var viewModel = new WorkTypeListViewModel
        {
            WorkTypes = result.Data!.Select(w => new WorkTypeDetailsViewModel
            {
                Code = w.Code,
                Name = w.Name,
                Description = w.Description,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt
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
        
        return View(new CreateWorkTypeViewModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkTypeViewModel model)
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
        
        var command = new CreateWorkTypeCommand
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
        
        var result = await _workTypeService.CreateWorkTypeAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = result.Messages.Any() ? result.Messages.First() : "Work type created successfully";
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
        
        var result = await _workTypeService.GetWorkTypeByCodeAsync(code);
        
        if (!result.Success)
        {
            TempData["Error"] = string.Join(", ", result.Errors);
            return RedirectToAction(nameof(Index));
        }
        
        var viewModel = new UpdateWorkTypeViewModel
        {
            Code = result.Data!.Code,
            Name = result.Data.Name,
            Description = result.Data.Description,
            IsActive = result.Data.IsActive
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(UpdateWorkTypeViewModel model)
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
        
        var command = new UpdateWorkTypeCommand
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
        
        var result = await _workTypeService.UpdateWorkTypeAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = result.Messages.Any() ? result.Messages.First() : "Work type updated successfully";
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
