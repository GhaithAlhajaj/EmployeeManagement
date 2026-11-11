
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.API.Authorization;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();
        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        // Create roles if they do not exist.
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role)
                );

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role: {role}"
                    );
                }
            }
        }

        // Promote the configured account to Admin.
        var adminEmail = configuration["SeedAdmin:Email"];

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            if (environment.IsProduction() &&
                (await userManager.GetUsersInRoleAsync(AppRoles.Admin)).Count == 0)
            {
                throw new InvalidOperationException(
                    "Configure SeedAdmin:Email and SeedAdmin:Password to bootstrap the first Admin account.");
            }

            return;
        }

        adminEmail = adminEmail.Trim().ToLowerInvariant();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var adminPassword = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "SeedAdmin:Password is required to create the initial Admin account.");
            }

            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = configuration["SeedAdmin:FirstName"]?.Trim() is { Length: > 0 } firstName
                    ? firstName
                    : "System",
                LastName = configuration["SeedAdmin:LastName"]?.Trim() is { Length: > 0 } lastName
                    ? lastName
                    : "Administrator",
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the configured Admin account: {string.Join(" ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(
            adminUser,
            AppRoles.Admin))
        {
            var result = await userManager.AddToRoleAsync(
                adminUser,
                AppRoles.Admin
            );

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role."
                );
            }
        }

        foreach (var role in new[] { AppRoles.Employee, AppRoles.HR })
        {
            if (await userManager.IsInRoleAsync(adminUser, role))
            {
                var result = await userManager.RemoveFromRoleAsync(adminUser, role);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to remove the {role} role from the configured Admin account.");
                }
            }
        }
    }

}
