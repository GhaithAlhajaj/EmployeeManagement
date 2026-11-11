namespace EmployeeManagement.MVC.ViewModels;

public class EmployeeDirectoryViewModel
{
    public List<EmployeeViewModel> Employees { get; set; } = [];

    public List<DepartmentViewModel> Departments { get; set; } = [];

    public string? Search { get; set; }

    public int? DepartmentId { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int TotalCount { get; set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class PagedEmployeesResponseViewModel
{
    public List<EmployeeViewModel> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}
