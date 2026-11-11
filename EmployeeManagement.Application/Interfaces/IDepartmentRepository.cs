using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync();

    Task<Department?> GetByIdAsync(int id);

    Task AddAsync(Department department);

    Task UpdateAsync(Department department);

    Task DeleteAsync(Department department);

    Task<bool> NameExistsAsync(string name);
}