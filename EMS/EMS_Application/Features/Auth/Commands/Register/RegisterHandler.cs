using EMS_Application.Common;
using EMS_Application.DTO.Auth;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.AppUsers;
using EMS_Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace EMS_Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public RegisterHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.AppUsers.FindAsync(u => u.Email == request.Email);

        if (existingUser is not null)
            throw new BadRequestException("A user with this email already exists.");

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _unitOfWork.AppUsers.AddAsync(user);
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
