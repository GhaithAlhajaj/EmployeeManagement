
using System.Net.Http.Json;
using System.Security.Claims;
using EmployeeManagement.MVC.Models.Account;
using EmployeeManagement.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyAccount()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EmployeeManagementAPI");
            var profile = await client.GetFromJsonAsync<MyAccountViewModel>("api/usermanagement/me");

            return profile == null
                ? Problem("Unable to load your account details.")
                : View(profile);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Account details are temporarily unavailable.";
            return View(new MyAccountViewModel
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
            });
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(MyAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("MyAccount", model);
        }

        var client = _httpClientFactory.CreateClient("EmployeeManagementAPI");
        try
        {
            var response = await client.PutAsJsonAsync(
                "api/usermanagement/me",
                new { model.FirstName, model.LastName, model.PhoneNumber });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ApiErrorMessage.ReadAsync(response));
                return View("MyAccount", model);
            }

            await RefreshNameClaimAsync($"{model.FirstName.Trim()} {model.LastName.Trim()}");
            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction(nameof(MyAccount));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The account service is temporarily unavailable.");
            return View("MyAccount", model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["PasswordError"] = string.Join(
                " ",
                ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
            TempData["PasswordError"] = string.Join(
                " ",
                ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
            return RedirectToAction(nameof(MyAccount));
        }

        var api = _httpClientFactory.CreateClient("EmployeeManagementAPI");
        try
        {
            var response = await api.PostAsJsonAsync(
                "api/usermanagement/me/change-password",
                new { model.CurrentPassword, model.NewPassword });

            if (!response.IsSuccessStatusCode)
            {
                TempData["PasswordError"] = await ApiErrorMessage.ReadAsync(response);
            }
            else
            {
                TempData["PasswordSuccess"] = "Your password has been changed.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["PasswordError"] = "The account service is temporarily unavailable.";
        }

        return RedirectToAction(nameof(MyAccount));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateManagedUserViewModel());
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateManagedUserViewModel model)
    {
        if (!User.IsInRole("Admin") && model.Role != "Employee")
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("EmployeeManagementAPI");
        try
        {
            var response = await client.PostAsJsonAsync("api/usermanagement/users", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ApiErrorMessage.ReadAsync(response));
                return View(model);
            }

            TempData["Success"] = $"The {model.Role} account was created.";
            return RedirectToAction(nameof(CreateUser));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The account service is temporarily unavailable.");
            return View(model);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient(
            "EmployeeManagementAPI"
        );

        var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new
            {
                model.Email,
                model.Password
            }
        );

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password."
            );

            return View(model);
        }

        var authResponse =
            await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();

        if (authResponse == null ||
            string.IsNullOrWhiteSpace(authResponse.Token))
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to complete login."
            );

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authResponse.Email),
            new Claim(ClaimTypes.Name, authResponse.FullName),
            new Claim(ClaimTypes.Email, authResponse.Email)
        };

        foreach (var role in authResponse.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = authResponse.ExpiresAt.ToUniversalTime()
        };

        properties.StoreTokens(
            new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = authResponse.Token
                }
            }
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties
        );

        return RedirectToAction(
            "Index",
            "Employees"
        );
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction(nameof(Login));
    }

    private async Task RefreshNameClaimAsync(string fullName)
    {
        var authentication = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (authentication.Principal == null)
        {
            return;
        }

        var claims = authentication.Principal.Claims
            .Where(claim => claim.Type != ClaimTypes.Name)
            .ToList();
        claims.Add(new Claim(ClaimTypes.Name, fullName));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authentication.Properties);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
