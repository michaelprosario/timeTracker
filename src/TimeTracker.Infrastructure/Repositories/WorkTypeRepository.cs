using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class WorkTypeRepository : IWorkTypeRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public WorkTypeRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<WorkType?> GetByCodeAsync(string code)
    {
        return await _context.WorkTypes.FindAsync(code.ToUpper());
    }
    
    public async Task<IEnumerable<WorkType>> GetAllAsync()
    {
        return await _context.WorkTypes
            .OrderBy(w => w.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<WorkType>> GetActiveAsync()
    {
        return await _context.WorkTypes
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }
    
    public async Task AddAsync(WorkType workType)
    {
        await _context.WorkTypes.AddAsync(workType);
    }
    
    public Task UpdateAsync(WorkType workType)
    {
        _context.WorkTypes.Update(workType);
        return Task.CompletedTask;
    }
}
