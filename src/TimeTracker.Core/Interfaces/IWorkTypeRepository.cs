using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface IWorkTypeRepository
{
    Task<WorkType?> GetByCodeAsync(string code);
    Task<IEnumerable<WorkType>> GetAllAsync();
    Task<IEnumerable<WorkType>> GetActiveAsync();
    Task AddAsync(WorkType workType);
    Task UpdateAsync(WorkType workType);
}
