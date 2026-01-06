using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Queries;
using TimeTracker.Core.Services;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers;

public class ReportsController : BaseController
{
    private readonly TimeEntryService _timeEntryService;
    
    public ReportsController(TimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> ProjectTimeReport(Guid? timeSheetId = null)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var query = new GetProjectTimeReportQuery
        {
            UserId = userId,
            TimeSheetId = timeSheetId
        };
        
        var result = await _timeEntryService.GetProjectTimeReportAsync(query);
        
        if (!result.Success)
        {
            TempData["Error"] = string.Join(", ", result.Errors);
            return RedirectToAction("Index", "Home");
        }
        
        var viewModel = new ProjectTimeReportViewModel
        {
            TimeSheets = result.Data!.Select(ts => new TimeSheetReportViewModel
            {
                TimeSheetId = ts.TimeSheetId,
                StartDate = ts.StartDate,
                EndDate = ts.EndDate,
                Status = ts.Status,
                TotalHours = ts.TotalHours,
                Projects = ts.Projects.Select(p => new ProjectSummaryViewModel
                {
                    ProjectCode = p.ProjectCode,
                    ProjectName = p.ProjectName,
                    TotalHours = p.TotalHours,
                    TimeEntries = p.TimeEntries.Select(te => new TimeEntryDetailViewModel
                    {
                        Id = te.Id,
                        EntryDate = te.EntryDate,
                        Hours = te.Hours,
                        WorkTypeCode = te.WorkTypeCode,
                        WorkTypeName = te.WorkTypeName,
                        Notes = te.Notes
                    }).ToList()
                }).ToList()
            }).ToList()
        };
        
        return View(viewModel);
    }
}
