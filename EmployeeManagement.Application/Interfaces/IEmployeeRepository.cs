using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync();

    Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        string? search,
        int? departmentId,
        bool? isActive,
        int skip,
        int take);

    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId);

    Task<(int TotalEmployees, int ActiveEmployees, decimal AverageSalary)> GetStatisticsAsync();

    Task<Employee?> GetByIdAsync(int id);

    Task AddAsync(Employee employee);

    Task UpdateAsync(Employee employee);

    Task DeleteAsync(Employee employee);

    Task<bool> EmailExistsAsync(string email);
}