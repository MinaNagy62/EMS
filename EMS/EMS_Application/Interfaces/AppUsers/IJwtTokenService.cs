using EMS_Domain.Entities;

namespace EMS_Application.Interfaces.AppUsers;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
}
