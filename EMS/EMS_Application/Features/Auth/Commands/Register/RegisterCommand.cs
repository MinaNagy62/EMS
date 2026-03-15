using EMS_Application.DTO.Auth;
using MediatR;

namespace EMS_Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<AuthResponse>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
