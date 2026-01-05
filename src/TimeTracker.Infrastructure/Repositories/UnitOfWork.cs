using Microsoft.EntityFrameworkCore.Storage;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TimeTrackerDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(
        TimeTrackerDbContext context,
        IUserRepository users,
        ITimeSheetRepository timeSheets,
        ITimeEntryRepository timeEntries,
        IProjectRepository projects,
        IWorkTypeRepository workTypes)
    {
        _context = context;
        Users = users;
        TimeSheets = timeSheets;
        TimeEntries = timeEntries;
        Projects = projects;
        WorkTypes = workTypes;
    }
    
    public IUserRepository Users { get; }
    public ITimeSheetRepository TimeSheets { get; }
    public ITimeEntryRepository TimeEntries { get; }
    public IProjectRepository Projects { get; }
    public IWorkTypeRepository WorkTypes { get; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
