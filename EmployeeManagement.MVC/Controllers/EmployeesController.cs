
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.MVC.ViewModels;
using EmployeeManagement.MVC.Services;

namespace EmployeeManagement.MVC.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EmployeesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetApiClient()
    {
        return _httpClientFactory.CreateClient("EmployeeManagementAPI");
    }

    // GET: /Employees
    public async Task<IActionResult> Index(
        string? search,
        int? departmentId,
        bool? isActive,
        int page = 1,
        int pageSize = 20)
    {
        var client = GetApiClient();
        var model = new EmployeeDirectoryViewModel
        {
            Search = search,
            DepartmentId = departmentId,
            IsActive = isActive,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100)
        };

        try
        {
            var query = $"page={model.Page}&pageSize={model.PageSize}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += $"&search={Uri.EscapeDataString(search)}";
            }
            if (departmentId.HasValue)
            {
                query += $"&departmentId={departmentId.Value}";
            }
            if (isActive.HasValue)
            {
                query += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";
            }

            var result = await client.GetFromJsonAsync<PagedEmployeesResponseViewModel>(
                $"api/employees/search?{query}");
            if (result != null)
            {
                model.Employees = result.Items;
                model.Page = result.Page;
                model.PageSize = result.PageSize;
                model.TotalCount = result.TotalCount;
            }

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                model.Departments = await client.GetFromJsonAsync<List<DepartmentViewModel>>(
                    "api/departments") ?? [];
            }

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Employee search is temporarily unavailable.";
            return View(model);
        }
    }

    // GET: /Employees/Details/1
    public async Task<IActionResult> Details(int id)
    {
        var client = GetApiClient();

        try
        {
            var employee = await client.GetFromJsonAsync<EmployeeViewModel>(
                $"api/employees/{id}"
            );

            if (employee == null)
                return NotFound();

            return View(employee);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve employee details from the API.");
        }
    }

    // GET: /Employees/Create
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateEmployeeViewModel();

        await LoadDepartments(model.Departments);

        return View(model);
    }

    // POST: /Employees/Create
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments(model.Departments);
            return View(model);
        }

        var client = GetApiClient();

        try
        {
            var response = await client.PostAsJsonAsync("api/employees", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiErrorMessage.ReadAsync(response));

                await LoadDepartments(model.Departments);
                return View(model);
            }

            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the API.");
            await LoadDepartments(model.Departments);
            return View(model);
        }
    }

    // GET: /Employees/Edit/1
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Edit(int id)
    {
        var client = GetApiClient();

        try
        {
            var employee = await client.GetFromJsonAsync<EmployeeViewModel>(
                $"api/employees/{id}"
            );

            if (employee == null)
                return NotFound();

            var model = new UpdateEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                IsActive = employee.IsActive
            };

            await LoadDepartments(model.Departments);

            return View(model);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve employee information from the API.");
        }
    }

    // POST: /Employees/Edit/1
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UpdateEmployeeViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await LoadDepartments(model.Departments);
            return View(model);
        }

        var client = GetApiClient();

        try
        {
            var response = await client.PutAsJsonAsync(
                $"api/employees/{id}",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiErrorMessage.ReadAsync(response));

                await LoadDepartments(model.Departments);
                return View(model);
            }

            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the API.");
            await LoadDepartments(model.Departments);
            return View(model);
        }
    }

    // GET: /Employees/Delete/1
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = GetApiClient();

        try
        {
            var employee = await client.GetFromJsonAsync<EmployeeViewModel>(
                $"api/employees/{id}"
            );

            if (employee == null)
                return NotFound();

            return View(employee);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve employee information from the API.");
        }
    }

    // POST: /Employees/Delete/1
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin,HR")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = GetApiClient();

        try
        {
            var response = await client.DeleteAsync($"api/employees/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await ApiErrorMessage.ReadAsync(response);
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Employee deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to connect to the employee service.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadDepartments(
        List<DepartmentViewModel> departments)
    {
        var client = GetApiClient();

        try
        {
            var result = await client.GetFromJsonAsync<List<DepartmentViewModel>>(
                "api/departments"
            );

            departments.Clear();

            if (result != null)
            {
                departments.AddRange(
                    result.Where(d => d.IsActive)
                );
            }
        }
        catch (HttpRequestException)
        {
            departments.Clear();
            ModelState.AddModelError(
                "",
                "Unable to load departments. Please try again."
            );
        }
    }
}
