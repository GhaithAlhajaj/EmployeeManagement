
using EmployeeManagement.API.Authorization;
using EmployeeManagement.Application.DTOs.Authentication;
using EmployeeManagement.Infrastructure.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("users")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.HR)]
    public async Task<IActionResult> CreateUser(
        CreateManagedUserRequest request)
    {
        var role = request.Role.Trim();
        var isAdmin = User.IsInRole(AppRoles.Admin);

        if (role != AppRoles.Employee &&
            !(isAdmin && role == AppRoles.HR))
        {
            return Forbid();
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid role",
                detail: "The selected role is unavailable.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) != null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Account validation failed",
                detail: string.Join(" ", createResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Role assignment failed",
                detail: string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        return Ok(new UserProfileResponse(user.Email!, user.FirstName, user.LastName, user.PhoneNumber ?? string.Empty));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var user = await GetCurrentUserAsync();
        return user == null
            ? NotFound()
            : Ok(new UserProfileResponse(user.Email!, user.FirstName, user.LastName, user.PhoneNumber ?? string.Empty));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateMyProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Ok(new UserProfileResponse(user.Email!, user.FirstName, user.LastName, user.PhoneNumber))
            : Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Profile update failed",
                detail: string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangeMyPassword(
        ChangeMyPasswordRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        return result.Succeeded
            ? NoContent()
            : Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Password change failed",
                detail: string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> AssignRole(
        [FromQuery, Required, EmailAddress] string email,
        [FromQuery, Required] string role)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "User not found.");
        }

        if (role != AppRoles.Admin &&
            role != AppRoles.HR &&
            role != AppRoles.Employee)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid role",
                detail: "The selected role is invalid.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count == 1 && currentRoles.Contains(role))
        {
            return Ok(new { message = $"Role {role} is already assigned." });
        }

        var addedRole = !currentRoles.Contains(role);
        if (addedRole)
        {
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Role assignment failed",
                    detail: string.Join(" ", addResult.Errors.Select(error => error.Description)));
            }
        }

        var removedRoles = new List<string>();
        foreach (var existingRole in currentRoles.Where(existingRole => existingRole != role))
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, existingRole);
            if (removeResult.Succeeded)
            {
                removedRoles.Add(existingRole);
                continue;
            }

            foreach (var removedRole in removedRoles)
            {
                await _userManager.AddToRoleAsync(user, removedRole);
            }
            if (addedRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Role assignment failed",
                detail: string.Join(" ", removeResult.Errors.Select(error => error.Description)));
        }

        return Ok(new
        {
            message = $"Role {role} assigned successfully."
        });
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");

        return string.IsNullOrWhiteSpace(email)
            ? null
            : await _userManager.FindByEmailAsync(email);
    }
}
