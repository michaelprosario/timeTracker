namespace TimeTracker.Web.Models;

public class ProjectTimeReportViewModel
{
    public List<TimeSheetReportViewModel> TimeSheets { get; set; } = new();
}

public class TimeSheetReportViewModel
{
    public Guid TimeSheetId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
}

public class ProjectSummaryViewModel
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<TimeEntryDetailViewModel> TimeEntries { get; set; } = new();
    public List<DailyTotalViewModel> DailyTotals { get; set; } = new();
}

public class TimeEntryDetailViewModel
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string WorkTypeCode { get; set; } = string.Empty;
    public string WorkTypeName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class DailyTotalViewModel
{
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
}
