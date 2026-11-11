using EmployeeManagement.Application.DTOs.Employees;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();

        return employees.Select(MapToDto).ToList();
    }

    public async Task<PagedEmployeesDto> SearchAsync(EmployeeSearchRequest request)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var (items, totalCount) = await _employeeRepository.SearchAsync(
            request.Search,
            request.DepartmentId,
            request.IsActive,
            skip,
            request.PageSize);

        return new PagedEmployeesDto
        {
            Items = items.Select(MapToDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetByDepartmentAsync(int departmentId)
    {
        var employees = await _employeeRepository.GetByDepartmentAsync(departmentId);
        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeStatisticsDto> GetStatisticsAsync()
    {
        var (total, active, averageSalary) = await _employeeRepository.GetStatisticsAsync();
        return new EmployeeStatisticsDto
        {
            TotalEmployees = total,
            ActiveEmployees = active,
            InactiveEmployees = total - active,
            AverageSalary = averageSalary
        };
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            return null;
        }

        return MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeDto employeeDto)
    {
        ValidateEmployeeDates(
            employeeDto.DateOfBirth,
            employeeDto.HireDate);

        var email = NormalizeEmail(employeeDto.Email);

        if (await _employeeRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException(
                "An employee with this email already exists.");
        }

        var department = await _departmentRepository.GetByIdAsync(
            employeeDto.DepartmentId);

        if (department == null)
        {
            throw new InvalidOperationException(
                "The selected department does not exist.");
        }

        var employee = new Employee
        {
            FirstName = employeeDto.FirstName.Trim(),
            LastName = employeeDto.LastName.Trim(),
            Email = email,
            PhoneNumber = employeeDto.PhoneNumber.Trim(),
            DateOfBirth = employeeDto.DateOfBirth,
            HireDate = employeeDto.HireDate,
            Salary = employeeDto.Salary,
            IsActive = true,
            DepartmentId = department.Id,
            Department = department
        };

        await _employeeRepository.AddAsync(employee);

        return MapToDto(employee);
    }

    public async Task<EmployeeDto?> UpdateAsync(
        int id,
        UpdateEmployeeDto employeeDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            return null;
        }

        ValidateEmployeeDates(
            employeeDto.DateOfBirth,
            employeeDto.HireDate);

        var email = NormalizeEmail(employeeDto.Email);

        if (!string.Equals(
                employee.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmailExistsAsync(email))
            {
                throw new InvalidOperationException(
                    "An employee with this email already exists.");
            }
        }

        var department = await _departmentRepository.GetByIdAsync(
            employeeDto.DepartmentId);

        if (department == null)
        {
            throw new InvalidOperationException(
                "The selected department does not exist.");
        }

        employee.FirstName = employeeDto.FirstName.Trim();
        employee.LastName = employeeDto.LastName.Trim();
        employee.Email = email;
        employee.PhoneNumber = employeeDto.PhoneNumber.Trim();
        employee.DateOfBirth = employeeDto.DateOfBirth;
        employee.HireDate = employeeDto.HireDate;
        employee.Salary = employeeDto.Salary;
        employee.IsActive = employeeDto.IsActive;
        employee.DepartmentId = department.Id;
        employee.Department = department;

        await _employeeRepository.UpdateAsync(employee);

        return MapToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            return false;
        }

        await _employeeRepository.DeleteAsync(employee);

        return true;
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidateEmployeeDates(
        DateTime dateOfBirth,
        DateTime hireDate)
    {
        if (dateOfBirth == default || dateOfBirth.Date > DateTime.Today)
        {
            throw new InvalidOperationException(
                "Date of birth must be a valid date in the past.");
        }

        if (hireDate == default)
        {
            throw new InvalidOperationException(
                "Hire date is required.");
        }
    }
}