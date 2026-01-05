using TimeTracker.Core.Commands;
using TimeTracker.Core.Common;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Queries;

namespace TimeTracker.Core.Services;

public class TimeSheetService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public TimeSheetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<AppResult<TimeSheet>> CreateOrGetTimeSheetAsync(CreateOrGetTimeSheetCommand command)
    {
        var existingTimeSheet = await _unitOfWork.TimeSheets.GetByUserIdAndDateAsync(command.UserId, command.ForDate);
        if (existingTimeSheet != null)
        {
            return AppResult<TimeSheet>.SuccessResult(existingTimeSheet);
        }
        
        var startDate = GetMondayOfWeek(command.ForDate);
        var endDate = startDate.AddDays(13);
        
        if (await _unitOfWork.TimeSheets.ExistsForPeriodAsync(command.UserId, startDate, endDate))
        {
            return AppResult<TimeSheet>.FailureResult("A timesheet already exists for this period");
        }
        
        var timeSheet = new TimeSheet
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            StartDate = startDate,
            EndDate = endDate,
            Status = TimeSheetStatus.Open,
            TotalHours = 0,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.TimeSheets.AddAsync(timeSheet);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<TimeSheet>.SuccessResult(timeSheet, "Timesheet created successfully");
    }
    
    public async Task<AppResult<TimeSheet>> CloseTimeSheetAsync(CloseTimeSheetCommand command)
    {
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(command.Id);
        if (timeSheet == null)
        {
            return AppResult<TimeSheet>.FailureResult("Timesheet not found");
        }
        
        if (timeSheet.UserId != command.UserId)
        {
            return AppResult<TimeSheet>.FailureResult("Unauthorized access");
        }
        
        if (timeSheet.Status == TimeSheetStatus.Closed)
        {
            return AppResult<TimeSheet>.FailureResult("Timesheet is already closed");
        }
        
        timeSheet.Status = TimeSheetStatus.Closed;
        timeSheet.LastModifiedAt = DateTime.UtcNow;
        timeSheet.CalculateTotalHours();
        
        await _unitOfWork.TimeSheets.UpdateAsync(timeSheet);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<TimeSheet>.SuccessResult(timeSheet, "Timesheet closed successfully");
    }
    
    public async Task<AppResult<IEnumerable<TimeSheet>>> GetUserTimeSheetsAsync(GetUserTimeSheetsQuery query)
    {
        var timeSheets = await _unitOfWork.TimeSheets.GetByUserIdAsync(query.UserId);
        return AppResult<IEnumerable<TimeSheet>>.SuccessResult(timeSheets);
    }
    
    public async Task<AppResult<TimeSheet>> GetTimeSheetByIdAsync(GetTimeSheetByIdQuery query)
    {
        var timeSheet = await _unitOfWork.TimeSheets.GetByIdAsync(query.Id);
        if (timeSheet == null)
        {
            return AppResult<TimeSheet>.FailureResult("Timesheet not found");
        }
        
        if (timeSheet.UserId != query.UserId)
        {
            return AppResult<TimeSheet>.FailureResult("Unauthorized access");
        }
        
        return AppResult<TimeSheet>.SuccessResult(timeSheet);
    }
    
    private DateTime GetMondayOfWeek(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek - 1 + 7) % 7;
        return date.Date.AddDays(-daysFromMonday);
    }
}
