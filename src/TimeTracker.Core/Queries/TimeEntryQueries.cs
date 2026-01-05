namespace TimeTracker.Core.Queries;

public class GetTimeEntriesByTimeSheetQuery
{
    public Guid TimeSheetId { get; set; }
    public Guid UserId { get; set; }
}

public class GetProjectsQuery
{
    public bool ActiveOnly { get; set; } = true;
}

public class GetWorkTypesQuery
{
    public bool ActiveOnly { get; set; } = true;
}
