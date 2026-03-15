using EMS_Application.Common;
using EMS_Application.DTO.Auth;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.AppUsers;
using MediatR;
using Microsoft.Extensions.Options;

namespace EMS_Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.FindAsync(
            u => u.RefreshToken == request.RefreshToken && u.IsActive);

        if (user is null)
            throw new NotFoundException("User", "invalid refresh token");

        if (user.RefreshTokenExpiryDate < DateTime.UtcNow)
            throw new BadRequestException("Refresh token has expired. Please login again.");

        var (newAccessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        _unitOfWork.AppUsers.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
