namespace EmployeeManagement.MVC.ViewModels;

public class DepartmentEmployeeReportViewModel
{
    public DepartmentViewModel Department { get; set; } = new();

    public List<EmployeeViewModel> Employees { get; set; } = [];
}
