using EMS_Application.DTO.Auth;
using MediatR;

namespace EMS_Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; }
    public string Password { get; set; }
}
