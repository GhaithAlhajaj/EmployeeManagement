using System.Diagnostics;
using System.Net.Http.Json;
using EmployeeManagement.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.MVC.Models;

namespace EmployeeManagement.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EmployeeManagementAPI");
            var statistics = await client.GetFromJsonAsync<EmployeeStatisticsViewModel>(
                "api/employees/statistics");
            return View(statistics ?? new EmployeeStatisticsViewModel());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Dashboard statistics are temporarily unavailable.";
            return View(new EmployeeStatisticsViewModel());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
