using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeEntry>> GetByTimeSheetIdAsync(Guid timeSheetId);
    Task<IEnumerable<TimeEntry>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task AddAsync(TimeEntry timeEntry);
    Task UpdateAsync(TimeEntry timeEntry);
    Task DeleteAsync(Guid id);
}
