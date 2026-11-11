using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        string? search,
        int? departmentId,
        bool? isActive,
        int skip,
        int take)
    {
        var query = _context.Employees
            .Include(employee => employee.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(employee =>
                employee.FirstName.ToLower().Contains(term) ||
                employee.LastName.ToLower().Contains(term) ||
                employee.Email.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(employee => employee.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(employee => employee.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .Include(employee => employee.Department)
            .AsNoTracking()
            .Where(employee => employee.DepartmentId == departmentId)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync();
    }

    public async Task<(int TotalEmployees, int ActiveEmployees, decimal AverageSalary)> GetStatisticsAsync()
    {
        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.CountAsync(employee => employee.IsActive);
        var averageSalary = totalEmployees == 0
            ? 0
            : (decimal)await _context.Employees.AverageAsync(employee => (double)employee.Salary);

        return (totalEmployees, activeEmployees, averageSalary);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Employee employee)
    {
        _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == email.ToLower());
    }
}