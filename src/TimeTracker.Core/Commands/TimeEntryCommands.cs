namespace TimeTracker.Core.Commands;

public class CreateTimeEntryCommand
{
    public Guid TimeSheetId { get; set; }
    public Guid UserId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string WorkTypeCode { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class UpdateTimeEntryCommand
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string WorkTypeCode { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Hours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class DeleteTimeEntryCommand
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
