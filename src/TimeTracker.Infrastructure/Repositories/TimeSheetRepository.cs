using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class TimeSheetRepository : ITimeSheetRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public TimeSheetRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<TimeSheet?> GetByIdAsync(Guid id)
    {
        return await _context.TimeSheets
            .Include(t => t.TimeEntries)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<IEnumerable<TimeSheet>> GetByUserIdAsync(Guid userId)
    {
        return await _context.TimeSheets
            .Where(t => t.UserId == userId)
            .Include(t => t.TimeEntries)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }
    
    public async Task<TimeSheet?> GetByUserIdAndDateAsync(Guid userId, DateTime date)
    {
        return await _context.TimeSheets
            .Where(t => t.UserId == userId && t.StartDate <= date && t.EndDate >= date)
            .Include(t => t.TimeEntries)
            .FirstOrDefaultAsync();
    }
    
    public async Task AddAsync(TimeSheet timeSheet)
    {
        await _context.TimeSheets.AddAsync(timeSheet);
    }
    
    public Task UpdateAsync(TimeSheet timeSheet)
    {
        _context.TimeSheets.Update(timeSheet);
        return Task.CompletedTask;
    }
    
    public async Task<bool> ExistsForPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.TimeSheets
            .AnyAsync(t => t.UserId == userId && 
                          t.StartDate == startDate && 
                          t.EndDate == endDate);
    }
}
