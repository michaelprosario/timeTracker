using TimeTracker.Core.Commands;
using TimeTracker.Core.Common;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Queries;

namespace TimeTracker.Core.Services;

public class WorkTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public WorkTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<AppResult<WorkType>> CreateWorkTypeAsync(CreateWorkTypeCommand command)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            validationErrors.Add(nameof(command.Code), new List<string> { "Work type code is required" });
        }
        
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            validationErrors.Add(nameof(command.Name), new List<string> { "Work type name is required" });
        }
        
        if (validationErrors.Any())
        {
            return AppResult<WorkType>.ValidationFailure(validationErrors);
        }
        
        var existingWorkType = await _unitOfWork.WorkTypes.GetByCodeAsync(command.Code);
        if (existingWorkType != null)
        {
            return AppResult<WorkType>.FailureResult("Work type code already exists");
        }
        
        var workType = new WorkType
        {
            Code = command.Code.ToUpper(),
            Name = command.Name,
            Description = command.Description,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.WorkTypes.AddAsync(workType);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<WorkType>.SuccessResult(workType, "Work type created successfully");
    }
    
    public async Task<AppResult<WorkType>> UpdateWorkTypeAsync(UpdateWorkTypeCommand command)
    {
        var validationErrors = new Dictionary<string, List<string>>();
        
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            validationErrors.Add(nameof(command.Code), new List<string> { "Work type code is required" });
        }
        
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            validationErrors.Add(nameof(command.Name), new List<string> { "Work type name is required" });
        }
        
        if (validationErrors.Any())
        {
            return AppResult<WorkType>.ValidationFailure(validationErrors);
        }
        
        var workType = await _unitOfWork.WorkTypes.GetByCodeAsync(command.Code);
        if (workType == null)
        {
            return AppResult<WorkType>.FailureResult("Work type not found");
        }
        
        workType.Name = command.Name;
        workType.Description = command.Description;
        workType.IsActive = command.IsActive;
        
        await _unitOfWork.WorkTypes.UpdateAsync(workType);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<WorkType>.SuccessResult(workType, "Work type updated successfully");
    }
    
    public async Task<AppResult<WorkType>> GetWorkTypeByCodeAsync(string code)
    {
        var workType = await _unitOfWork.WorkTypes.GetByCodeAsync(code);
        if (workType == null)
        {
            return AppResult<WorkType>.FailureResult("Work type not found");
        }
        
        return AppResult<WorkType>.SuccessResult(workType);
    }
    
    public async Task<AppResult<IEnumerable<WorkType>>> GetWorkTypesAsync(GetWorkTypesQuery query)
    {
        var workTypes = query.ActiveOnly
            ? await _unitOfWork.WorkTypes.GetActiveAsync()
            : await _unitOfWork.WorkTypes.GetAllAsync();
        
        return AppResult<IEnumerable<WorkType>>.SuccessResult(workTypes);
    }
}
