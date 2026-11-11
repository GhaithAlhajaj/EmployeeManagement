namespace EmployeeManagement.Application.DTOs.Employees;

public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public DateTime HireDate { get; set; }

    public decimal Salary { get; set; }

    public bool IsActive { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;
}