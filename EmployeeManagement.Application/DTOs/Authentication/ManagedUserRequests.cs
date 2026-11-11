using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.DTOs.Authentication;

public sealed record CreateManagedUserRequest
{
    [Required, StringLength(50)]
    public required string FirstName { get; init; }

    [Required, StringLength(50)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(8)]
    public required string Password { get; init; }

    [Required]
    public required string Role { get; init; }
}

public sealed record UpdateMyProfileRequest
{
    [Required, StringLength(50)]
    public required string FirstName { get; init; }

    [Required, StringLength(50)]
    public required string LastName { get; init; }

    [StringLength(20)]
    public string PhoneNumber { get; init; } = string.Empty;
}

public sealed record ChangeMyPasswordRequest
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required, MinLength(8)]
    public required string NewPassword { get; init; }
}

public sealed record UserProfileResponse(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber);