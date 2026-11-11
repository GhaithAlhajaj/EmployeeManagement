using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeManagement.Application.DTOs.Authentication;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Tests;

public sealed class ApiAuthorizationTests : IAsyncLifetime
{
    private EmployeeManagementApiFactory _factory = null!;
    private HttpClient _client = null!;
    private readonly Dictionary<string, string> _tokens = new();
    private const string Password = "Integration@12345";

    public async Task InitializeAsync()
    {
        _factory = new EmployeeManagementApiFactory();
        _client = _factory.CreateClient();

        await CreateUserAsync("admin@example.test", "Admin");
        await CreateUserAsync("hr@example.test", "HR");
        await CreateUserAsync("employee@example.test", "Employee");

        foreach (var email in new[]
                 {
                     "admin@example.test",
                     "hr@example.test",
                     "employee@example.test"
                 })
        {
            var login = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new { email, password = Password });
            login.EnsureSuccessStatusCode();

            var response = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
            _tokens[email] = response!.Token;
        }
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task ProtectedEndpoints_EnforceExpectedRoleMatrix()
    {
        var anonymousResponse = await _client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal("application/problem+json", anonymousResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);

        var registrationResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { firstName = "Public", lastName = "User", email = "public@example.test", password = Password });
        Assert.Equal(HttpStatusCode.NotFound, registrationResponse.StatusCode);

        var admin = AuthorizedClient("admin@example.test");
        var hr = AuthorizedClient("hr@example.test");
        var employee = AuthorizedClient("employee@example.test");
        var invalidSearch = await admin.GetAsync("/api/employees/search?pageSize=0");
        Assert.Equal(HttpStatusCode.BadRequest, invalidSearch.StatusCode);
        Assert.Equal("application/problem+json", invalidSearch.Content.Headers.ContentType?.MediaType);

        var roleClients = new Dictionary<string, HttpClient>
        {
            ["Admin"] = admin,
            ["HR"] = hr,
            ["Employee"] = employee
        };

        foreach (var (role, client) in roleClients)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/employees")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/employees/search?page=1&pageSize=10")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/employees/9999")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/WeatherForecast")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/usermanagement/me")).StatusCode);
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                (await client.GetAsync("/api/employees/statistics")).StatusCode);

            var updateProfile = await client.PutAsJsonAsync(
                "/api/usermanagement/me",
                new { firstName = role, lastName = "Updated", phoneNumber = "555-0101" });
            Assert.Equal(HttpStatusCode.OK, updateProfile.StatusCode);
            var updatedProfile = await updateProfile.Content.ReadFromJsonAsync<UserProfileResponse>();
            Assert.Equal("555-0101", updatedProfile!.PhoneNumber);

            var updatedPassword = $"Updated-{role}@12345";
            var changePassword = await client.PostAsJsonAsync(
                "/api/usermanagement/me/change-password",
                new { currentPassword = Password, newPassword = updatedPassword });
            Assert.Equal(HttpStatusCode.NoContent, changePassword.StatusCode);
            var updatedLogin = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new { email = $"{role.ToLowerInvariant()}@example.test", password = updatedPassword });
            Assert.Equal(HttpStatusCode.OK, updatedLogin.StatusCode);

            var employeeCreate = await client.PostAsJsonAsync("/api/employees", new { });
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.BadRequest,
                employeeCreate.StatusCode);

            var employeeUpdate = await client.PutAsJsonAsync("/api/employees/9999", new { });
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.BadRequest,
                employeeUpdate.StatusCode);

            var employeeDelete = await client.DeleteAsync("/api/employees/9999");
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.NotFound,
                employeeDelete.StatusCode);

            var departments = await client.GetAsync("/api/departments");
            var departmentById = await client.GetAsync("/api/departments/9999");
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                departments.StatusCode);
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.NotFound,
                departmentById.StatusCode);
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.NotFound,
                (await client.GetAsync("/api/departments/9999/employees")).StatusCode);

            var departmentCreate = await client.PostAsJsonAsync(
                "/api/departments",
                new { name = $"Integration {role}", description = "Test department" });
            Assert.Equal(role == "Admin" ? HttpStatusCode.Created : HttpStatusCode.Forbidden,
                departmentCreate.StatusCode);

            var departmentUpdate = await client.PutAsJsonAsync(
                "/api/departments/9999",
                new { name = "Updated department", description = "Test", isActive = true });
            Assert.Equal(role == "Admin" ? HttpStatusCode.NotFound : HttpStatusCode.Forbidden,
                departmentUpdate.StatusCode);

            var departmentDelete = await client.DeleteAsync("/api/departments/9999");
            Assert.Equal(role == "Admin" ? HttpStatusCode.NotFound : HttpStatusCode.Forbidden,
                departmentDelete.StatusCode);

            var createManagedUser = await client.PostAsJsonAsync(
                "/api/usermanagement/users",
                new { firstName = "Created", lastName = role, email = $"new-{role.ToLowerInvariant()}@example.test", password = Password, role = "Employee" });
            Assert.Equal(role == "Employee" ? HttpStatusCode.Forbidden : HttpStatusCode.OK,
                createManagedUser.StatusCode);
        }

        var adminCreateHr = await admin.PostAsJsonAsync(
            "/api/usermanagement/users",
            new { firstName = "New", lastName = "HR", email = "admin-created-hr@example.test", password = Password, role = "HR" });
        Assert.Equal(HttpStatusCode.OK, adminCreateHr.StatusCode);

        var hrCreateHr = await hr.PostAsJsonAsync(
            "/api/usermanagement/users",
            new { firstName = "Not", lastName = "Allowed", email = "hr-cannot-create-hr@example.test", password = Password, role = "HR" });
        Assert.Equal(HttpStatusCode.Forbidden, hrCreateHr.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/audit")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await hr.GetAsync("/api/audit")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await employee.GetAsync("/api/audit")).StatusCode);

        var roleAssignment = await admin.PostAsync(
            "/api/usermanagement/assign-role?email=employee%40example.test&role=HR",
            content: null);
        Assert.Equal(HttpStatusCode.OK, roleAssignment.StatusCode);

        var replaceRole = await admin.PostAsync(
            "/api/usermanagement/assign-role?email=employee%40example.test&role=Employee",
            content: null);
        Assert.Equal(HttpStatusCode.OK, replaceRole.StatusCode);
        var roleRefresh = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "employee@example.test", password = "Updated-Employee@12345" });
        Assert.Equal(HttpStatusCode.OK, roleRefresh.StatusCode);
        var refreshedAuth = await roleRefresh.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.Equal(new[] { "Employee" }, refreshedAuth!.Roles);

        var hrRoleAssignment = await hr.PostAsync(
            "/api/usermanagement/assign-role?email=employee%40example.test&role=Admin",
            content: null);
        Assert.Equal(HttpStatusCode.Forbidden, hrRoleAssignment.StatusCode);

        var employeeRoleAssignment = await employee.PostAsync(
            "/api/usermanagement/assign-role?email=hr%40example.test&role=Admin",
            content: null);
        Assert.Equal(HttpStatusCode.Forbidden, employeeRoleAssignment.StatusCode);

        admin.Dispose();
        hr.Dispose();
        employee.Dispose();
    }

    private HttpClient AuthorizedClient(string email)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _tokens[email]);
        return client;
    }

    private async Task CreateUserAsync(string email, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = role,
            LastName = "User",
            IsActive = true
        };

        var create = await userManager.CreateAsync(user, Password);
        Assert.True(create.Succeeded, string.Join("; ", create.Errors.Select(error => error.Description)));
        var assign = await userManager.AddToRoleAsync(user, role);
        Assert.True(assign.Succeeded, string.Join("; ", assign.Errors.Select(error => error.Description)));
    }
}

internal sealed class EmployeeManagementApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly Dictionary<string, string?> _originalEnvironment = new();

    public EmployeeManagementApiFactory()
    {
        SetEnvironment("Jwt__Key", "Integration-tests-key-with-more-than-thirty-two-characters");
        SetEnvironment("Jwt__Issuer", "EmployeeManagement.Tests");
        SetEnvironment("Jwt__Audience", "EmployeeManagement.Tests.Client");
        SetEnvironment("Jwt__ExpirationMinutes", "30");
        SetEnvironment("ConnectionStrings__DefaultConnection", "Data Source=:memory:");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>()
                .Database.Migrate();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
            foreach (var (key, value) in _originalEnvironment)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private void SetEnvironment(string key, string value)
    {
        _originalEnvironment[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
