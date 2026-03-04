using EMS_Application.DTO.Auth;
using EMS_Domain.Entities;

namespace EMS_Application.Interfaces.AppUsers;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
