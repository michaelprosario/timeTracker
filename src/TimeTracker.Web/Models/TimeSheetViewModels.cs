namespace TimeTracker.Web.Models;

public class TimeSheetViewModel
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
}

public class TimeSheetListViewModel
{
    public List<TimeSheetViewModel> TimeSheets { get; set; } = new();
}

public class TimeSheetDetailsViewModel
{
    public TimeSheetViewModel TimeSheet { get; set; } = new();
    public List<TimeEntryViewModel> TimeEntries { get; set; } = new();
    public List<ProjectViewModel> AvailableProjects { get; set; } = new();
    public List<WorkTypeViewModel> AvailableWorkTypes { get; set; } = new();
}

public class TimeEntryViewModel
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string WorkTypeCode { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ProjectViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class WorkTypeViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
