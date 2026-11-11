using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.MVC.ViewModels;

public class UpdateEmployeeViewModel
{
    public int Id { get; set; }

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

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [Range(0.01, 10000000)]
    public decimal Salary { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public List<DepartmentViewModel> Departments { get; set; } = new();
}