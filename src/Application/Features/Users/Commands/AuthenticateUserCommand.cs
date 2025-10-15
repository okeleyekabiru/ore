using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Users.Commands;

public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresOnUtc,
    DateTime RefreshTokenExpiresOnUtc,
    Guid UserId,
    string Email,
    string FullName,
    RoleType Role,
    Guid? TeamId);

public sealed record AuthenticateUserCommand(string Email, string Password) : IRequest<Result<AuthenticationResponse>>;

public sealed class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<AuthenticationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthenticateUserCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        IJwtTokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
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

        if (!user.IsActive)
        {
            return Result<AuthenticationResponse>.Failure("User account is inactive");
        }

        var accessTokenResult = _tokenService.GenerateAccessToken(user.Id, user.Email, user.FullName, authResult.Roles, user.TeamId);
        var refreshTokenResult = _tokenService.GenerateRefreshToken();

        var utcNow = _dateTimeProvider.UtcNow;
        var refreshTokenEntity = RefreshToken.Create(user.Id, TokenHashHelper.ComputeHash(refreshTokenResult.Token), refreshTokenResult.ExpiresOnUtc, utcNow);
        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthenticationResponse(
            accessTokenResult.Token,
            refreshTokenResult.Token,
            accessTokenResult.ExpiresOnUtc,
            refreshTokenResult.ExpiresOnUtc,
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.TeamId);

        return Result<AuthenticationResponse>.Success(response);
    }

}
