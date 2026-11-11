using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs.Departments;

public class CreateDepartmentDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}