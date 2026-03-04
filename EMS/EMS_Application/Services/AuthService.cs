using EMS_Application.Common;
using EMS_Application.DTO.Auth;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.AppUsers;
using EMS_Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EMS_Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

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

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

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

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _unitOfWork.AppUsers.FindAsync(
            u => u.RefreshToken == refreshToken && u.IsActive);

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
