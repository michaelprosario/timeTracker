using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITimeSheetRepository
{
    Task<TimeSheet?> GetByIdAsync(Guid id);
    Task<IEnumerable<TimeSheet>> GetByUserIdAsync(Guid userId);
    Task<TimeSheet?> GetByUserIdAndDateAsync(Guid userId, DateTime date);
    Task AddAsync(TimeSheet timeSheet);
    Task UpdateAsync(TimeSheet timeSheet);
    Task<bool> ExistsForPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
}
