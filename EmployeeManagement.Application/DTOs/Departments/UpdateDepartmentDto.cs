using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs.Departments;

public class UpdateDepartmentDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}