using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Api.Contracts.Auth;
using Ore.Application.Common.Models;
using Ore.Application.Features.Users.Commands;
using Ore.Domain.Enums;

namespace Ore.Api.Controllers;

public sealed class AuthController : ApiControllerBase
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request.BrandSurvey is null)
        {
            return FromResult(Result<Guid>.Failure("Brand survey onboarding details are required."));
        }

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role,
            request.TeamName,
            request.IsIndividual,
            new BrandSurveyOnboardingInput(
                request.BrandSurvey.Voice,
                request.BrandSurvey.Tone,
                request.BrandSurvey.Audience,
                request.BrandSurvey.Goals,
                request.BrandSurvey.Competitors,
                request.BrandSurvey.Keywords ?? Array.Empty<string>()));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "User registered successfully.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AuthenticateUserCommand(request.Email, request.Password), cancellationToken);
        return FromResult(result, "Authentication successful.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RefreshAccessTokenCommand(request.RefreshToken), cancellationToken);
        return FromResult(result, "Token refreshed successfully.");
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = nameof(RoleType.Admin))]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<RoleType>(request.Role, ignoreCase: true, out var role))
        {
            return FromResult(Result<Guid>.Failure("Unsupported role value provided."));
        }

        if (request.UserId == Guid.Empty)
        {
            return FromResult(Result<Guid>.Failure("UserId must be provided."));
        }

        var command = new AssignRoleCommand(request.UserId, role);
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "User role updated.");
    }
}
