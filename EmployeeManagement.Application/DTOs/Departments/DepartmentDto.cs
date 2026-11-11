namespace EmployeeManagement.Application.DTOs.Departments;

public class DepartmentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int EmployeeCount { get; set; }
}