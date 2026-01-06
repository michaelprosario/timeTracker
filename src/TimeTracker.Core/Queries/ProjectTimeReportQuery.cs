namespace TimeTracker.Core.Queries;

public class GetProjectTimeReportQuery
{
    public Guid UserId { get; set; }
    public Guid? TimeSheetId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProjectTimeReportDto
{
    public Guid TimeSheetId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<ProjectTimeSummaryDto> Projects { get; set; } = new();
}

public class ProjectTimeSummaryDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public List<TimeEntryDetailDto> TimeEntries { get; set; } = new();
}

public class TimeEntryDetailDto
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string WorkTypeCode { get; set; } = string.Empty;
    public string WorkTypeName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
