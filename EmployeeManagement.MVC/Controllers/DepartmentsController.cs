
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.MVC.ViewModels;
using EmployeeManagement.MVC.Services;

namespace EmployeeManagement.MVC.Controllers;

[Authorize(Roles = "Admin,HR")]
public class DepartmentsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DepartmentsController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetApiClient()
    {
        return _httpClientFactory.CreateClient(
            "EmployeeManagementAPI"
        );
    }

    // GET: /Departments
    public async Task<IActionResult> Index()
    {
        var client = GetApiClient();

        try
        {
            var departments =
                await client.GetFromJsonAsync<List<DepartmentViewModel>>(
                    "api/departments"
                );

            return View(
                departments ?? new List<DepartmentViewModel>()
            );
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load departments from the API.";

            return View(new List<DepartmentViewModel>());
        }
    }

    // GET: /Departments/Details/1
    public async Task<IActionResult> Details(int id)
    {
        var client = GetApiClient();

        try
        {
            var department =
                await client.GetFromJsonAsync<DepartmentViewModel>(
                    $"api/departments/{id}"
                );

            if (department == null)
                return NotFound();

            return View(department);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve department details.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmployeeReport(int id)
    {
        var client = GetApiClient();

        try
        {
            var department = await client.GetFromJsonAsync<DepartmentViewModel>(
                $"api/departments/{id}");
            if (department == null)
            {
                return NotFound();
            }

            var employees = await client.GetFromJsonAsync<List<EmployeeViewModel>>(
                $"api/departments/{id}/employees");

            return View(new DepartmentEmployeeReportViewModel
            {
                Department = department,
                Employees = employees ?? []
            });
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The department report is temporarily unavailable.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // GET: /Departments/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new DepartmentViewModel());
    }

    // POST: /Departments/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = GetApiClient();

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/departments",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiErrorMessage.ReadAsync(response));

                return View(model);
            }

            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "",
                "Unable to connect to the API."
            );

            return View(model);
        }
    }

    // GET: /Departments/Edit/1
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var client = GetApiClient();

        try
        {
            var department =
                await client.GetFromJsonAsync<DepartmentViewModel>(
                    $"api/departments/{id}"
                );

            if (department == null)
                return NotFound();

            return View(department);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve department information.");
        }
    }

    // POST: /Departments/Edit/1
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        DepartmentViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var client = GetApiClient();

        try
        {
            var response = await client.PutAsJsonAsync(
                $"api/departments/{id}",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiErrorMessage.ReadAsync(response));

                return View(model);
            }

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "",
                "Unable to connect to the API."
            );

            return View(model);
        }
    }

    // GET: /Departments/Delete/1
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = GetApiClient();

        try
        {
            var department =
                await client.GetFromJsonAsync<DepartmentViewModel>(
                    $"api/departments/{id}"
                );

            if (department == null)
                return NotFound();

            return View(department);
        }
        catch (HttpRequestException)
        {
            return Problem("Unable to retrieve department information.");
        }
    }

    // POST: /Departments/Delete/1
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = GetApiClient();

        try
        {
            var response = await client.DeleteAsync(
                $"api/departments/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await ApiErrorMessage.ReadAsync(response);
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Department deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to connect to the department service.";
            return RedirectToAction(nameof(Index));
        }
    }
}
