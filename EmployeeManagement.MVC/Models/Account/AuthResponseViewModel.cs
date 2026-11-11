
namespace EmployeeManagement.MVC.Models.Account;

public class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}
