namespace TimeTracker.Core.Entities;

public class Project
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
