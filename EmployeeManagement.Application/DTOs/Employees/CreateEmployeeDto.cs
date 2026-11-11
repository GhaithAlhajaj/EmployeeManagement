using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs.Employees;

public class CreateEmployeeDto : IValidatableObject
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [Range(0.01, 100000000)]
    public decimal Salary { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DateOfBirth == default || DateOfBirth.Date > DateTime.Today)
        {
            yield return new ValidationResult(
                "Date of birth must be a valid date in the past.",
                [nameof(DateOfBirth)]);
        }

        if (HireDate == default)
        {
            yield return new ValidationResult(
                "Hire date is required.",
                [nameof(HireDate)]);
        }
        else if (DateOfBirth != default && HireDate.Date < DateOfBirth.Date)
        {
            yield return new ValidationResult(
                "Hire date cannot be before date of birth.",
                [nameof(HireDate)]);
        }
    }
}