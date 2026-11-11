namespace EmployeeManagement.MVC.ViewModels;

public class EmployeeStatisticsViewModel
{
    public int TotalEmployees { get; set; }

    public int ActiveEmployees { get; set; }

    public int InactiveEmployees { get; set; }

    public decimal AverageSalary { get; set; }
}
