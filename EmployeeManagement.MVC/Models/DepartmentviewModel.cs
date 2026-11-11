
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.MVC.ViewModels;

public class DepartmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public int EmployeeCount { get; set; }
}
