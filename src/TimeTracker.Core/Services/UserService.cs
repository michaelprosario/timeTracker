using System.Security.Cryptography;
using System.Text;
using TimeTracker.Core.Commands;
using TimeTracker.Core.Common;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Core.Services;

public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<AppResult<User>> RegisterUserAsync(RegisterUserCommand command)
    {
        var validationErrors = ValidateRegistration(command);
        if (validationErrors.Any())
        {
            return AppResult<User>.ValidationFailure(validationErrors);
        }
        
        if (await _unitOfWork.Users.EmailExistsAsync(command.Email))
        {
            return AppResult<User>.FailureResult("Email already exists");
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLower(),
            PasswordHash = HashPassword(command.Password),
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        return AppResult<User>.SuccessResult(user, "User registered successfully");
    }
    
    public async Task<AppResult<User>> LoginAsync(LoginUserCommand command)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(command.Email.ToLower());
        if (user == null)
        {
            return AppResult<User>.FailureResult("Invalid email or password");
        }
        
        if (!VerifyPassword(command.Password, user.PasswordHash))
        {
            return AppResult<User>.FailureResult("Invalid email or password");
        }
        
        if (!user.IsActive)
        {
            return AppResult<User>.FailureResult("Account is inactive");
        }
        
        return AppResult<User>.SuccessResult(user, "Login successful");
    }
    
    private Dictionary<string, List<string>> ValidateRegistration(RegisterUserCommand command)
    {
        var errors = new Dictionary<string, List<string>>();
        
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors.Add(nameof(command.Email), new List<string> { "Email is required" });
        }
        else if (!IsValidEmail(command.Email))
        {
            errors.Add(nameof(command.Email), new List<string> { "Email format is invalid" });
        }
        
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors.Add(nameof(command.Password), new List<string> { "Password is required" });
        }
        else if (!IsValidPassword(command.Password))
        {
            errors.Add(nameof(command.Password), new List<string> { 
                "Password must be at least 8 characters, contain uppercase, lowercase, number, and special character" 
            });
        }
        
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors.Add(nameof(command.FirstName), new List<string> { "First name is required" });
        }
        
        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors.Add(nameof(command.LastName), new List<string> { "Last name is required" });
        }
        
        return errors;
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private bool IsValidPassword(string password)
    {
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
    
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
