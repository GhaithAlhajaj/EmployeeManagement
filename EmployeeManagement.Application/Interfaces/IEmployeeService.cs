using EmployeeManagement.Application.DTOs.Employees;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync();

    Task<PagedEmployeesDto> SearchAsync(EmployeeSearchRequest request);

    Task<IReadOnlyList<EmployeeDto>> GetByDepartmentAsync(int departmentId);

    Task<EmployeeStatisticsDto> GetStatisticsAsync();

    Task<EmployeeDto?> GetByIdAsync(int id);

    Task<EmployeeDto> CreateAsync(CreateEmployeeDto employeeDto);

    Task<EmployeeDto?> UpdateAsync(
        int id,
        UpdateEmployeeDto employeeDto);

    Task<bool> DeleteAsync(int id);
}