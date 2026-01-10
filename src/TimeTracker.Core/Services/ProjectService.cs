using TimeTracker.Core.Commands;
using TimeTracker.Core.Common;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Queries;

namespace TimeTracker.Core.Services;

public class ProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<AppResult<Project>> CreateProjectAsync(CreateProjectCommand command)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            validationErrors.Add(nameof(command.Code), new List<string> { "Project code is required" });
        }
        
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            validationErrors.Add(nameof(command.Name), new List<string> { "Project name is required" });
        }
        
        if (validationErrors.Any())
        {
            return AppResult<Project>.ValidationFailure(validationErrors);
        }
        
        var existingProject = await _unitOfWork.Projects.GetByCodeAsync(command.Code);
        if (existingProject != null)
        {
            return AppResult<Project>.FailureResult("Project code already exists");
        }
        
        var project = new Project
        {
            Code = command.Code.ToUpper(),
            Name = command.Name,
            Description = command.Description,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<Project>.SuccessResult(project, "Project created successfully");
    }
    
    public async Task<AppResult<Project>> UpdateProjectAsync(UpdateProjectCommand command)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            validationErrors.Add(nameof(command.Code), new List<string> { "Project code is required" });
        }
        
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            validationErrors.Add(nameof(command.Name), new List<string> { "Project name is required" });
        }
        
        if (validationErrors.Any())
        {
            return AppResult<Project>.ValidationFailure(validationErrors);
        }
        
        var project = await _unitOfWork.Projects.GetByCodeAsync(command.Code);
        if (project == null)
        {
            return AppResult<Project>.FailureResult("Project not found");
        }
        
        project.Name = command.Name;
        project.Description = command.Description;
        project.IsActive = command.IsActive;
        
        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<Project>.SuccessResult(project, "Project updated successfully");
    }
    
    public async Task<AppResult<Project>> GetProjectByCodeAsync(string code)
    {
        var project = await _unitOfWork.Projects.GetByCodeAsync(code);
        if (project == null)
        {
            return AppResult<Project>.FailureResult("Project not found");
        }
        
        return AppResult<Project>.SuccessResult(project);
    }
    
    public async Task<AppResult<IEnumerable<Project>>> GetProjectsAsync(GetProjectsQuery query)
    {
        var projects = query.ActiveOnly
            ? await _unitOfWork.Projects.GetActiveAsync()
            : await _unitOfWork.Projects.GetAllAsync();
        
        return AppResult<IEnumerable<Project>>.SuccessResult(projects);
    }
}
