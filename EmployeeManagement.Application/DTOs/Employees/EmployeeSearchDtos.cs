using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs.Employees;

public class EmployeeSearchRequest
{
    [StringLength(100)]
    public string? Search { get; set; }

    [Range(1, 1000000)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    [Range(1, int.MaxValue)]
    public int? DepartmentId { get; set; }

    public bool? IsActive { get; set; }
}

public class PagedEmployeesDto
{
    public IReadOnlyList<EmployeeDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

public class EmployeeStatisticsDto
{
    public int TotalEmployees { get; set; }

    public int ActiveEmployees { get; set; }

    public int InactiveEmployees { get; set; }

    public decimal AverageSalary { get; set; }
}
