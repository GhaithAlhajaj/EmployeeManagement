
namespace EmployeeManagement.API.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string HR = "HR";
    public const string Employee = "Employee";

    public static readonly string[] All =
    {
        Admin,
        HR,
        Employee
    };
}
