using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly TimeTrackerDbContext _context;
    
    public ProjectRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }
    
    public async Task<Project?> GetByCodeAsync(string code)
    {
        return await _context.Projects.FindAsync(code.ToUpper());
    }
    
    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Project>> GetActiveAsync()
    {
        return await _context.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }
    
    public Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        return Task.CompletedTask;
    }
}
