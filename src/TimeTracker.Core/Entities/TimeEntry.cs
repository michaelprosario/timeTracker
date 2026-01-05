namespace TimeTracker.Core.Entities;

public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid TimeSheetId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string WorkTypeCode { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal Hours { get; set; }
    public DateTime EntryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime LastModifiedAt { get; set; }
    
    public TimeSheet? TimeSheet { get; set; }
    public Project? Project { get; set; }
    public WorkType? WorkType { get; set; }
}
