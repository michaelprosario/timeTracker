using NSubstitute;
using NUnit.Framework;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;

namespace TimeTracker.Core.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private IUnitOfWork _unitOfWork;
    private IUserRepository _userRepository;
    private UserService _userService;
    
    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork.Users.Returns(_userRepository);
        _userService = new UserService(_unitOfWork);
    }
    
    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }
    
    [Test]
    public async Task RegisterUserAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        _unitOfWork.SaveChangesAsync().Returns(1);
        
        // Act
        var result = await _userService.RegisterUserAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Email, Is.EqualTo("test@example.com"));
        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
    
    [Test]
    public async Task RegisterUserAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(true);
        
        // Act
        var result = await _userService.RegisterUserAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Email already exists"));
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
    
    [Test]
    public async Task RegisterUserAsync_WithInvalidPassword_ReturnsValidationFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "weak",
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        var result = await _userService.RegisterUserAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ValidationErrors, Does.ContainKey(nameof(command.Password)));
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
    
    [Test]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        
        var hashedPassword = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(command.Password)
            )
        );
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };
        
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        
        // Act
        var result = await _userService.LoginAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Email, Is.EqualTo("test@example.com"));
    }
    
    [Test]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };
        
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        
        // Act
        var result = await _userService.LoginAsync(command);
        
        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors, Contains.Item("Invalid email or password"));
    }
}
