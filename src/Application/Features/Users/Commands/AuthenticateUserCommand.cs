using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Users.Commands;

public sealed record AuthenticationResponse(string Token, Guid UserId, string Email, string FullName, RoleType Role, Guid? TeamId);

public sealed record AuthenticateUserCommand(string Email, string Password) : IRequest<Result<AuthenticationResponse>>;

public sealed class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _tokenService;

    public AuthenticateUserCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        IJwtTokenService tokenService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.AuthenticateAsync(request.Email, request.Password, cancellationToken);

        if (authResult is null)
        {
            return Result<AuthenticationResponse>.Failure("Invalid credentials");
        }

        var user = await _dbContext.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == authResult.UserId, cancellationToken);

        if (user is null)
        {
            return Result<AuthenticationResponse>.Failure("User profile not found");
        }

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.FullName, authResult.Roles, user.TeamId);

        var response = new AuthenticationResponse(
            token,
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.TeamId);

        return Result<AuthenticationResponse>.Success(response);
    }
}
