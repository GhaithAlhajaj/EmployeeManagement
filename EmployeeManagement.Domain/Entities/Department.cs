
namespace EmployeeManagement.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    // Navigation Property
    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
