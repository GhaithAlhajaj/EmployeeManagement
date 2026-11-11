using System.Net.Http.Json;
using EmployeeManagement.MVC.Services;
using EmployeeManagement.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AuditLogsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuditLogsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);

        try
        {
            var client = _httpClientFactory.CreateClient("EmployeeManagementAPI");
            var entries = await client.GetFromJsonAsync<List<AuditLogViewModel>>(
                $"api/audit?limit={limit}");
            return View(entries ?? []);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Audit history is temporarily unavailable.";
            return View(new List<AuditLogViewModel>());
        }
    }
}
