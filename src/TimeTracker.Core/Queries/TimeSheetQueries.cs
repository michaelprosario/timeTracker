namespace TimeTracker.Core.Queries;

public class GetUserTimeSheetsQuery
{
    public Guid UserId { get; set; }
}

public class GetTimeSheetByIdQuery
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class GetTimeSheetForDateQuery
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
}
