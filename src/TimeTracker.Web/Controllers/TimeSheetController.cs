using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Queries;
using TimeTracker.Core.Services;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class TimeSheetController : BaseController
{
    private readonly TimeSheetService _timeSheetService;
    private readonly TimeEntryService _timeEntryService;
    
    public TimeSheetController(TimeSheetService timeSheetService, TimeEntryService timeEntryService)
    {
        _timeSheetService = timeSheetService;
        _timeEntryService = timeEntryService;
    }
    
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var query = new GetUserTimeSheetsQuery { UserId = userId };
        var result = await _timeSheetService.GetUserTimeSheetsAsync(query);
        
        var viewModel = new TimeSheetListViewModel
        {
            TimeSheets = result.Data!.Select(ts => new TimeSheetViewModel
            {
                Id = ts.Id,
                StartDate = ts.StartDate,
                EndDate = ts.EndDate,
                Status = ts.Status.ToString(),
                TotalHours = ts.TotalHours
            }).ToList()
        };
        
        return View(viewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var query = new GetTimeSheetByIdQuery { Id = id, UserId = userId };
        var result = await _timeSheetService.GetTimeSheetByIdAsync(query);
        
        if (!result.Success)
        {
            TempData["Error"] = string.Join(", ", result.Errors);
            return RedirectToAction(nameof(Index));
        }
        
        var entriesQuery = new GetTimeEntriesByTimeSheetQuery { TimeSheetId = id, UserId = userId };
        var entriesResult = await _timeEntryService.GetTimeEntriesByTimeSheetAsync(entriesQuery);
        
        var projectsResult = await _timeEntryService.GetProjectsAsync(new GetProjectsQuery { ActiveOnly = true });
        var workTypesResult = await _timeEntryService.GetWorkTypesAsync(new GetWorkTypesQuery { ActiveOnly = true });
        
        var viewModel = new TimeSheetDetailsViewModel
        {
            TimeSheet = new TimeSheetViewModel
            {
                Id = result.Data!.Id,
                StartDate = result.Data.StartDate,
                EndDate = result.Data.EndDate,
                Status = result.Data.Status.ToString(),
                TotalHours = result.Data.TotalHours
            },
            TimeEntries = entriesResult.Data!.Select(te => new TimeEntryViewModel
            {
                Id = te.Id,
                ProjectCode = te.ProjectCode,
                WorkTypeCode = te.WorkTypeCode,
                EntryDate = te.EntryDate,
                Hours = te.Hours,
                Notes = te.Notes
            }).ToList(),
            AvailableProjects = projectsResult.Data!.Select(p => new ProjectViewModel
            {
                Code = p.Code,
                Name = p.Name
            }).ToList(),
            AvailableWorkTypes = workTypesResult.Data!.Select(w => new WorkTypeViewModel
            {
                Code = w.Code,
                Name = w.Name
            }).ToList()
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateEntry(Guid timeSheetId, TimeEntryViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var command = new CreateTimeEntryCommand
        {
            TimeSheetId = timeSheetId,
            UserId = userId,
            ProjectCode = model.ProjectCode,
            WorkTypeCode = model.WorkTypeCode,
            EntryDate = model.EntryDate,
            Hours = model.Hours,
            Notes = model.Notes ?? string.Empty
        };
        
        var result = await _timeEntryService.CreateTimeEntryAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = "Time entry added successfully";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors);
        }
        
        return RedirectToAction(nameof(Details), new { id = timeSheetId });
    }
    
    [HttpPost]
    public async Task<IActionResult> CloseTimeSheet(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var command = new CloseTimeSheetCommand { Id = id, UserId = userId };
        var result = await _timeSheetService.CloseTimeSheetAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = "Timesheet closed successfully";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors);
        }
        
        return RedirectToAction(nameof(Details), new { id });
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteEntry(Guid id, Guid timeSheetId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var command = new DeleteTimeEntryCommand { Id = id, UserId = userId };
        var result = await _timeEntryService.DeleteTimeEntryAsync(command);
        
        if (result.Success)
        {
            TempData["Success"] = "Time entry deleted successfully";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors);
        }
        
        return RedirectToAction(nameof(Details), new { id = timeSheetId });
    }
}
