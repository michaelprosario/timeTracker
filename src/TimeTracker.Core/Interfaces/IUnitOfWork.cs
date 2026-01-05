namespace TimeTracker.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITimeSheetRepository TimeSheets { get; }
    ITimeEntryRepository TimeEntries { get; }
    IProjectRepository Projects { get; }
    IWorkTypeRepository WorkTypes { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
