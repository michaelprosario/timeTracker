namespace TimeTracker.Core.Commands;

public class CreateOrGetTimeSheetCommand
{
    public Guid UserId { get; set; }
    public DateTime ForDate { get; set; }
}

public class CloseTimeSheetCommand
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
