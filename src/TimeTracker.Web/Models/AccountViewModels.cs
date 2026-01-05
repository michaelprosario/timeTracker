using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Web.Models;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
