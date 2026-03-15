using EMS_Application.Common;
using EMS_Application.DTO.Auth;
using EMS_Application.Features.Auth.Commands.Login;
using EMS_Application.Features.Auth.Commands.RefreshToken;
using EMS_Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully."));
    }
}
