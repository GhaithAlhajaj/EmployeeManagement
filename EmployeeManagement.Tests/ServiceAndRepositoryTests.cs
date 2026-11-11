using EmployeeManagement.Application.DTOs.Departments;
using EmployeeManagement.Application.DTOs.Employees;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Tests;

public class ServiceAndRepositoryTests
{
    [Fact]
    public async Task EmployeeService_NormalizesEmailAndRejectsDuplicateEmailCaseInsensitively()
    {
        var employees = new FakeEmployeeRepository();
        var departments = new FakeDepartmentRepository();
        departments.Add(new Department { Id = 1, Name = "Engineering" });
        var service = new EmployeeService(employees, departments);

        var created = await service.CreateAsync(new CreateEmployeeDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = " ADA@example.com ",
            DateOfBirth = new DateTime(1980, 1, 1),
            HireDate = new DateTime(2020, 1, 1),
            Salary = 90000,
            DepartmentId = 1
        });

        Assert.Equal("ada@example.com", created.Email);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new CreateEmployeeDto
            {
                FirstName = "Another",
                LastName = "User",
                Email = "ADA@example.com",
                DateOfBirth = new DateTime(1985, 1, 1),
                HireDate = new DateTime(2021, 1, 1),
                Salary = 70000,
                DepartmentId = 1
            }));
    }

    [Fact]
    public async Task DepartmentService_RejectsDuplicateNameIgnoringCase()
    {
        var departments = new FakeDepartmentRepository();
        departments.Add(new Department { Id = 1, Name = "Human Resources" });
        var service = new DepartmentService(departments);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new CreateDepartmentDto { Name = " human resources " }));
    }

    [Fact]
    public void EmployeeRequest_RejectsHireDateBeforeBirthDate()
    {
        var request = new CreateEmployeeDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            HireDate = new DateTime(1989, 1, 1),
            Salary = 90000,
            DepartmentId = 1
        };
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            request,
            new System.ComponentModel.DataAnnotations.ValidationContext(request),
            errors,
            validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.HireDate)));
    }

    [Fact]
    public async Task Repositories_CheckDuplicateValuesIgnoringCase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var department = new Department { Name = "Operations" };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var employee = new Employee
        {
            FirstName = "Grace",
            LastName = "Hopper",
            Email = "grace@example.com",
            DepartmentId = department.Id,
            Department = department,
            DateOfBirth = new DateTime(1980, 1, 1),
            HireDate = new DateTime(2020, 1, 1),
            Salary = 80000
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var employeeRepository = new EmployeeRepository(context);
        var departmentRepository = new DepartmentRepository(context);

        Assert.True(await employeeRepository.EmailExistsAsync("GRACE@EXAMPLE.COM"));
        Assert.True(await departmentRepository.NameExistsAsync("operations"));

        var page = await employeeRepository.SearchAsync(
            "grace",
            department.Id,
            true,
            skip: 0,
            take: 10);
        Assert.Single(page.Items);
        Assert.Equal(1, page.TotalCount);
        Assert.Single(await employeeRepository.GetByDepartmentAsync(department.Id));

        var statistics = await employeeRepository.GetStatisticsAsync();
        Assert.Equal(1, statistics.TotalEmployees);
        Assert.Equal(1, statistics.ActiveEmployees);
        Assert.Equal(80000, statistics.AverageSalary);
    }

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employees = [];

        public Task<IReadOnlyList<Employee>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<Employee>>(_employees);

        public Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
            string? search,
            int? departmentId,
            bool? isActive,
            int skip,
            int take)
        {
            IEnumerable<Employee> filtered = _employees;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(employee =>
                    employee.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    employee.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            if (departmentId.HasValue)
            {
                filtered = filtered.Where(employee => employee.DepartmentId == departmentId.Value);
            }
            if (isActive.HasValue)
            {
                filtered = filtered.Where(employee => employee.IsActive == isActive.Value);
            }

            var items = filtered.ToList();
            return Task.FromResult<(IReadOnlyList<Employee>, int)>((items.Skip(skip).Take(take).ToList(), items.Count));
        }

        public Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId) =>
            Task.FromResult<IReadOnlyList<Employee>>(_employees.Where(employee => employee.DepartmentId == departmentId).ToList());

        public Task<(int TotalEmployees, int ActiveEmployees, decimal AverageSalary)> GetStatisticsAsync()
        {
            var active = _employees.Count(employee => employee.IsActive);
            var average = _employees.Count == 0 ? 0 : _employees.Average(employee => employee.Salary);
            return Task.FromResult((_employees.Count, active, average));
        }

        public Task<Employee?> GetByIdAsync(int id) =>
            Task.FromResult(_employees.FirstOrDefault(employee => employee.Id == id));

        public Task AddAsync(Employee employee)
        {
            employee.Id = _employees.Count + 1;
            _employees.Add(employee);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Employee employee) => Task.CompletedTask;

        public Task DeleteAsync(Employee employee)
        {
            _employees.Remove(employee);
            return Task.CompletedTask;
        }

        public Task<bool> EmailExistsAsync(string email) =>
            Task.FromResult(_employees.Any(employee =>
                string.Equals(employee.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed class FakeDepartmentRepository : IDepartmentRepository
    {
        private readonly List<Department> _departments = [];

        public void Add(Department department) => _departments.Add(department);

        public Task<IReadOnlyList<Department>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<Department>>(_departments);

        public Task<Department?> GetByIdAsync(int id) =>
            Task.FromResult(_departments.FirstOrDefault(department => department.Id == id));

        public Task AddAsync(Department department)
        {
            department.Id = _departments.Count + 1;
            _departments.Add(department);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Department department) => Task.CompletedTask;

        public Task DeleteAsync(Department department)
        {
            _departments.Remove(department);
            return Task.CompletedTask;
        }

        public Task<bool> NameExistsAsync(string name) =>
            Task.FromResult(_departments.Any(department =>
                string.Equals(department.Name, name, StringComparison.OrdinalIgnoreCase)));
    }
}
