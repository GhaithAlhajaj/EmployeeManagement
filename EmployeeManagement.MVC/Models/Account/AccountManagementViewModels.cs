using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.MVC.Models.Account;

public class MyAccountViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Use at least 8 characters with uppercase and lowercase letters, a number, and a symbol.")]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CreateManagedUserViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Use at least 8 characters with uppercase and lowercase letters, a number, and a symbol.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";
}