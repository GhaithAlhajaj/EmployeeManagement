using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.Employees)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Department department)
    {
        _context.Departments.Remove(department);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Departments
            .AnyAsync(d => d.Name.ToLower() == name.ToLower());
    }
}