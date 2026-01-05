namespace TimeTracker.Core.Entities;

public enum TimeSheetStatus
{
    Open,
    Closed
}

public class TimeSheet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSheetStatus Status { get; set; }
    public decimal TotalHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    
    public User? User { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    
    public void CalculateTotalHours()
    {
        TotalHours = TimeEntries.Sum(e => e.Hours);
    }
    
    public bool IsDateWithinPeriod(DateTime date)
    {
        return date.Date >= StartDate.Date && date.Date <= EndDate.Date;
    }
}
