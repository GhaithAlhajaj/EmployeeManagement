using EmployeeManagement.Application.DTOs.Departments;

namespace EmployeeManagement.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync();

    Task<DepartmentDto?> GetByIdAsync(int id);

    Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto departmentDto);

    Task<DepartmentDto?> UpdateAsync(
        int id,
        UpdateDepartmentDto departmentDto);

    Task<bool> DeleteAsync(int id);
}