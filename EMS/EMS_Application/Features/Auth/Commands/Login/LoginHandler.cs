using EMS_Application.Common;
using EMS_Application.DTO.Auth;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.AppUsers;
using EMS_Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace EMS_Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.FindAsync(u => u.Email == request.Email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid email or password.");

        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        _unitOfWork.AppUsers.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
