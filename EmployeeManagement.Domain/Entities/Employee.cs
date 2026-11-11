
namespace EmployeeManagement.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }

    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime DateOfBirth { get; set; }
    // Foreign Key
    public int DepartmentId { get; set; }

    // Navigation Property
    public Department Department { get; set; } = null!;
}
