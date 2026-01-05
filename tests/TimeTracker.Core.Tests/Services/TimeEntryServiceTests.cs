using NSubstitute;
using NUnit.Framework;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;

namespace TimeTracker.Core.Tests.Services;

[TestFixture]
public class TimeEntryServiceTests
{
    private IUnitOfWork _unitOfWork;
    private ITimeSheetRepository _timeSheetRepository;
    private ITimeEntryRepository _timeEntryRepository;
    private IProjectRepository _projectRepository;
    private IWorkTypeRepository _workTypeRepository;
    private TimeEntryService _timeEntryService;
    
    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeSheetRepository = Substitute.For<ITimeSheetRepository>();
        _timeEntryRepository = Substitute.For<ITimeEntryRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _workTypeRepository = Substitute.For<IWorkTypeRepository>();
        
        _unitOfWork.TimeSheets.Returns(_timeSheetRepository);
        _unitOfWork.TimeEntries.Returns(_timeEntryRepository);
        _unitOfWork.Projects.Returns(_projectRepository);
        _unitOfWork.WorkTypes.Returns(_workTypeRepository);
        
        _timeEntryService = new TimeEntryService(_unitOfWork);
    }
    
    [Test]
    public async Task CreateTimeEntryAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var timeSheetId = Guid.NewGuid();
        var timeSheet = new TimeSheet
        {
            Id = timeSheetId,
            UserId = userId,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today.AddDays(6),
            Status = TimeSheetStatus.Open
        };
        
        var command = new CreateTimeEntryCommand
        {
            TimeSheetId = timeSheetId,
            UserId = userId,
            ProjectCode = "PROJECT-A",
            WorkTypeCode = "DEV",
            EntryDate = DateTime.Today,
            Hours = 8.0m,
            Notes = "Test work"
        };
        
        var project = new Project { Code = "PROJECT-A", Name = "Project A", IsActive = true };
        var workType = new WorkType { Code = "DEV", Name = "Development", IsActive = true };
        
        _timeSheetRepository.GetByIdAsync(timeSheetId).Returns(timeSheet);
        _projectRepository.GetByCodeAsync(Arg.Any<string>()).Returns(project);
        _workTypeRepository.GetByCodeAsync(Arg.Any<string>()).Returns(workType);
        _unitOfWork.SaveChangesAsync().Returns(1);
        
        // Act
        var result = await _timeEntryService.CreateTimeEntryAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Hours, Is.EqualTo(8.0m));
        await _timeEntryRepository.Received(1).AddAsync(Arg.Any<TimeEntry>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
    
    [Test]
    public async Task CreateTimeEntryAsync_WithClosedTimeSheet_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var timeSheetId = Guid.NewGuid();
        var timeSheet = new TimeSheet
        {
            Id = timeSheetId,
            UserId = userId,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today.AddDays(6),
            Status = TimeSheetStatus.Closed
        };
        
        var command = new CreateTimeEntryCommand
        {
            TimeSheetId = timeSheetId,
            UserId = userId,
            ProjectCode = "PROJECT-A",
            WorkTypeCode = "DEV",
            EntryDate = DateTime.Today,
            Hours = 8.0m
        };
        
        _timeSheetRepository.GetByIdAsync(timeSheetId).Returns(timeSheet);
        
        // Act
        var result = await _timeEntryService.CreateTimeEntryAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Cannot add entries to closed timesheet"));
        await _timeEntryRepository.DidNotReceive().AddAsync(Arg.Any<TimeEntry>());
    }
    
    [Test]
    public async Task CreateTimeEntryAsync_WithInvalidHours_ReturnsValidationFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var timeSheetId = Guid.NewGuid();
        var timeSheet = new TimeSheet
        {
            Id = timeSheetId,
            UserId = userId,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today.AddDays(6),
            Status = TimeSheetStatus.Open
        };
        
        var command = new CreateTimeEntryCommand
        {
            TimeSheetId = timeSheetId,
            UserId = userId,
            ProjectCode = "PROJECT-A",
            WorkTypeCode = "DEV",
            EntryDate = DateTime.Today,
            Hours = 25.0m // Invalid: more than 24 hours
        };
        
        _timeSheetRepository.GetByIdAsync(timeSheetId).Returns(timeSheet);
        
        // Act
        var result = await _timeEntryService.CreateTimeEntryAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ValidationErrors, Does.ContainKey(nameof(command.Hours)));
        await _timeEntryRepository.DidNotReceive().AddAsync(Arg.Any<TimeEntry>());
    }
}
