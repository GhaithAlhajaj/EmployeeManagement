using EmployeeManagement.Application.DTOs.Departments;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();

        return departments.Select(MapToDto).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
            return null;

        return MapToDto(department);
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto departmentDto)
    {
        var name = departmentDto.Name.Trim();

        if (await _departmentRepository.NameExistsAsync(name))
        {
            throw new InvalidOperationException(
                "A department with this name already exists.");
        }

        var department = new Department
        {
            Name = name,
            Description = departmentDto.Description.Trim(),
            IsActive = true
        };

        await _departmentRepository.AddAsync(department);

        return MapToDto(department);
    }

    public async Task<DepartmentDto?> UpdateAsync(
        int id,
        UpdateDepartmentDto departmentDto)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
            return null;

        var name = departmentDto.Name.Trim();

        if (!string.Equals(
            department.Name,
            name,
            StringComparison.OrdinalIgnoreCase))
        {
            if (await _departmentRepository.NameExistsAsync(name))
            {
                throw new InvalidOperationException(
                    "A department with this name already exists.");
            }
        }

        department.Name = name;
        department.Description = departmentDto.Description.Trim();
        department.IsActive = departmentDto.IsActive;

        await _departmentRepository.UpdateAsync(department);

        return MapToDto(department);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
            return false;

        if (department.Employees.Any())
        {
            throw new InvalidOperationException(
                "Cannot delete a department that has employees.");
        }

        await _departmentRepository.DeleteAsync(department);

        return true;
    }

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive,
            EmployeeCount = department.Employees.Count
        };
    }
}