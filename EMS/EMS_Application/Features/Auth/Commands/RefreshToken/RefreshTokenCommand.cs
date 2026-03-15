using EMS_Application.DTO.Auth;
using MediatR;

namespace EMS_Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public string RefreshToken { get; set; }
}
