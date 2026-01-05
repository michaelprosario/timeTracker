using TimeTracker.Core.Commands;
using TimeTracker.Core.Common;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Queries;

namespace TimeTracker.Core.Services;

public class TimeEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public TimeEntryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<AppResult<TimeEntry>> CreateTimeEntryAsync(CreateTimeEntryCommand command)
    {
        var validationResult = await ValidateTimeEntryAsync(command);
        if (!validationResult.Success)
        {
            return AppResult<TimeEntry>.ValidationFailure(validationResult.ValidationErrors);
        }
        
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(command.TimeSheetId);
        if (timeSheet == null || timeSheet.UserId != command.UserId)
        {
            return AppResult<TimeEntry>.FailureResult("Invalid timesheet");
        }
        
        if (timeSheet.Status == TimeSheetStatus.Closed)
        {
            return AppResult<TimeEntry>.FailureResult("Cannot add entries to closed timesheet");
        }
        
        if (!timeSheet.IsDateWithinPeriod(command.EntryDate))
        {
            return AppResult<TimeEntry>.FailureResult("Entry date must be within timesheet period");
        }
        
        var timeEntry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            TimeSheetId = command.TimeSheetId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.UserId,
            ProjectCode = command.ProjectCode.ToUpper(),
            WorkTypeCode = command.WorkTypeCode.ToUpper(),
            Hours = command.Hours,
            EntryDate = DateTime.SpecifyKind(command.EntryDate.Date, DateTimeKind.Utc),
            Notes = command.Notes ?? string.Empty,
            StartTime = command.StartTime.HasValue ? DateTime.SpecifyKind(command.StartTime.Value, DateTimeKind.Utc) : null,
            EndTime = command.EndTime.HasValue ? DateTime.SpecifyKind(command.EndTime.Value, DateTimeKind.Utc) : null,
            LastModifiedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.TimeEntries.AddAsync(timeEntry);
        
        timeSheet.LastModifiedAt = DateTime.UtcNow;
        timeSheet.CalculateTotalHours();
        await _unitOfWork.TimeSheets.UpdateAsync(timeSheet);
        
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<TimeEntry>.SuccessResult(timeEntry, "Time entry created successfully");
    }
    
    public async Task<AppResult<TimeEntry>> UpdateTimeEntryAsync(UpdateTimeEntryCommand command)
    {
        var timeEntry = await _unitOfWork.TimeEntries.GetByIdAsync(command.Id);
        if (timeEntry == null)
        {
            return AppResult<TimeEntry>.FailureResult("Time entry not found");
        }
        
        if (timeEntry.CreatedBy != command.UserId)
        {
            return AppResult<TimeEntry>.FailureResult("Unauthorized access");
        }
        
        var validationResult = await ValidateTimeEntryUpdateAsync(command);
        if (!validationResult.Success)
        {
            return AppResult<TimeEntry>.ValidationFailure(validationResult.ValidationErrors);
        }
        
        timeEntry.ProjectCode = command.ProjectCode.ToUpper();
        timeEntry.WorkTypeCode = command.WorkTypeCode.ToUpper();
        timeEntry.Hours = command.Hours;
        timeEntry.EntryDate = DateTime.SpecifyKind(command.EntryDate.Date, DateTimeKind.Utc);
        timeEntry.Notes = command.Notes ?? string.Empty;
        timeEntry.StartTime = command.StartTime.HasValue ? DateTime.SpecifyKind(command.StartTime.Value, DateTimeKind.Utc) : null;
        timeEntry.EndTime = command.EndTime.HasValue ? DateTime.SpecifyKind(command.EndTime.Value, DateTimeKind.Utc) : null;
        timeEntry.LastModifiedAt = DateTime.UtcNow;
        
        await _unitOfWork.TimeEntries.UpdateAsync(timeEntry);
        
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(timeEntry.TimeSheetId);
        if (timeSheet != null)
        {
            timeSheet.LastModifiedAt = DateTime.UtcNow;
            timeSheet.CalculateTotalHours();
            await _unitOfWork.TimeSheets.UpdateAsync(timeSheet);
        }
        
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<TimeEntry>.SuccessResult(timeEntry, "Time entry updated successfully");
    }
    
    public async Task<AppResult> DeleteTimeEntryAsync(DeleteTimeEntryCommand command)
    {
        var timeEntry = await _unitOfWork.TimeEntries.GetByIdAsync(command.Id);
        if (timeEntry == null)
        {
            return AppResult.FailureResult("Time entry not found");
        }
        
        if (timeEntry.CreatedBy != command.UserId)
        {
            return AppResult.FailureResult("Unauthorized access");
        }
        
        var timeSheetId = timeEntry.TimeSheetId;
        await _unitOfWork.TimeEntries.DeleteAsync(command.Id);
        
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(timeSheetId);
        if (timeSheet != null)
        {
            timeSheet.LastModifiedAt = DateTime.UtcNow;
            timeSheet.CalculateTotalHours();
            await _unitOfWork.TimeSheets.UpdateAsync(timeSheet);
        }
        
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult.SuccessResult("Time entry deleted successfully");
    }
    
    public async Task<AppResult<IEnumerable<TimeEntry>>> GetTimeEntriesByTimeSheetAsync(GetTimeEntriesByTimeSheetQuery query)
    {
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(query.TimeSheetId);
        if (timeSheet == null || timeSheet.UserId != query.UserId)
        {
            return AppResult<IEnumerable<TimeEntry>>.FailureResult("Invalid timesheet");
        }
        
        var entries = await _unitOfWork.TimeEntries.GetByTimeSheetIdAsync(query.TimeSheetId);
        return AppResult<IEnumerable<TimeEntry>>.SuccessResult(entries);
    }
    
    public async Task<AppResult<IEnumerable<Project>>> GetProjectsAsync(GetProjectsQuery query)
    {
        var projects = query.ActiveOnly
            ? await _unitOfWork.Projects.GetActiveAsync()
            : await _unitOfWork.Projects.GetAllAsync();
        
        return AppResult<IEnumerable<Project>>.SuccessResult(projects);
    }
    
    public async Task<AppResult<IEnumerable<WorkType>>> GetWorkTypesAsync(GetWorkTypesQuery query)
    {
        var workTypes = query.ActiveOnly
            ? await _unitOfWork.WorkTypes.GetActiveAsync()
            : await _unitOfWork.WorkTypes.GetAllAsync();
        
        return AppResult<IEnumerable<WorkType>>.SuccessResult(workTypes);
    }
    
    private async Task<AppResult> ValidateTimeEntryAsync(CreateTimeEntryCommand command)
    {
        var errors = new Dictionary<string, List<string>>();
        
        if (command.Hours <= 0 || command.Hours > 24)
        {
            errors.Add(nameof(command.Hours), new List<string> { "Hours must be between 0.25 and 24" });
        }
        
        if (string.IsNullOrWhiteSpace(command.ProjectCode))
        {
            errors.Add(nameof(command.ProjectCode), new List<string> { "Project code is required" });
        }
        else
        {
            var project = await _unitOfWork.Projects.GetByCodeAsync(command.ProjectCode.ToUpper());
            if (project == null)
            {
                errors.Add(nameof(command.ProjectCode), new List<string> { "Invalid project code" });
            }
            else if (!project.IsActive)
            {
                errors.Add(nameof(command.ProjectCode), new List<string> { "Project is inactive" });
            }
        }
        
        if (string.IsNullOrWhiteSpace(command.WorkTypeCode))
        {
            errors.Add(nameof(command.WorkTypeCode), new List<string> { "Work type code is required" });
        }
        else
        {
            var workType = await _unitOfWork.WorkTypes.GetByCodeAsync(command.WorkTypeCode.ToUpper());
            if (workType == null)
            {
                errors.Add(nameof(command.WorkTypeCode), new List<string> { "Invalid work type code" });
            }
            else if (!workType.IsActive)
            {
                errors.Add(nameof(command.WorkTypeCode), new List<string> { "Work type is inactive" });
            }
        }
        
        if (errors.Any())
        {
            return AppResult.ValidationFailure(errors);
        }
        
        return AppResult.SuccessResult();
    }
    
    private async Task<AppResult> ValidateTimeEntryUpdateAsync(UpdateTimeEntryCommand command)
    {
        var errors = new Dictionary<string, List<string>>();
        
        if (command.Hours <= 0 || command.Hours > 24)
        {
            errors.Add(nameof(command.Hours), new List<string> { "Hours must be between 0.25 and 24" });
        }
        
        if (string.IsNullOrWhiteSpace(command.ProjectCode))
        {
            errors.Add(nameof(command.ProjectCode), new List<string> { "Project code is required" });
        }
        else
        {
            var project = await _unitOfWork.Projects.GetByCodeAsync(command.ProjectCode.ToUpper());
            if (project == null)
            {
                errors.Add(nameof(command.ProjectCode), new List<string> { "Invalid project code" });
            }
        }
        
        if (string.IsNullOrWhiteSpace(command.WorkTypeCode))
        {
            errors.Add(nameof(command.WorkTypeCode), new List<string> { "Work type code is required" });
        }
        else
        {
            var workType = await _unitOfWork.WorkTypes.GetByCodeAsync(command.WorkTypeCode.ToUpper());
            if (workType == null)
            {
                errors.Add(nameof(command.WorkTypeCode), new List<string> { "Invalid work type code" });
            }
        }
        
        if (errors.Any())
        {
            return AppResult.ValidationFailure(errors);
        }
        
        return AppResult.SuccessResult();
    }
}
