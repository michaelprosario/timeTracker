using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public TimeEntryRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<TimeEntry?> GetByIdAsync(Guid id)
    {
        return await _context.TimeEntries
            .Include(t => t.Project)
            .Include(t => t.WorkType)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<IEnumerable<TimeEntry>> GetByTimeSheetIdAsync(Guid timeSheetId)
    {
        return await _context.TimeEntries
            .Where(t => t.TimeSheetId == timeSheetId)
            .Include(t => t.Project)
            .Include(t => t.WorkType)
            .OrderBy(t => t.EntryDate)
            .ThenBy(t => t.StartTime)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<TimeEntry>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.TimeEntries
            .Include(t => t.TimeSheet)
            .Where(t => t.TimeSheet!.UserId == userId && 
                       t.EntryDate >= startDate && 
                       t.EntryDate <= endDate)
            .Include(t => t.Project)
            .Include(t => t.WorkType)
            .OrderBy(t => t.EntryDate)
            .ToListAsync();
    }
    
    public async Task AddAsync(TimeEntry timeEntry)
    {
        await _context.TimeEntries.AddAsync(timeEntry);
    }
    
    public Task UpdateAsync(TimeEntry timeEntry)
    {
        _context.TimeEntries.Update(timeEntry);
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry != null)
        {
            _context.TimeEntries.Remove(timeEntry);
        }
    }
}
