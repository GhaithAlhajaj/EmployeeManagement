using EmployeeManagement.Application.DTOs.Authentication;

namespace EmployeeManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
