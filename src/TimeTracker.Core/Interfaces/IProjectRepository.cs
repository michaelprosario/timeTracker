using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByCodeAsync(string code);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetActiveAsync();
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
}
