using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Api.Contracts.Auth;
using Ore.Application.Common.Models;
using Ore.Application.Features.Users.Commands;

namespace Ore.Api.Controllers;

[AllowAnonymous]
public sealed class AuthController : ApiControllerBase
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role,
            request.TeamName,
            request.IsIndividual);

    var result = await Mediator.Send(command, cancellationToken);
    return FromResult(result, "User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AuthenticateUserCommand(request.Email, request.Password), cancellationToken);
        return FromResult(result, "Authentication successful.");
    }
}
