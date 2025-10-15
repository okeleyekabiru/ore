using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;

namespace Ore.Application.Features.Users.Commands;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<Result<AuthenticationResponse>>;

public sealed class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, Result<AuthenticationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshAccessTokenCommandHandler(
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

    public async Task<Result<AuthenticationResponse>> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = TokenHashHelper.ComputeHash(request.RefreshToken);
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, cancellationToken);

        if (refreshToken is null)
        {
            return Result<AuthenticationResponse>.Failure("Refresh token is invalid.");
        }

        var utcNow = _dateTimeProvider.UtcNow;
        if (!refreshToken.IsActive(utcNow))
        {
            return Result<AuthenticationResponse>.Failure("Refresh token is expired or revoked.");
        }

        var user = refreshToken.User ?? await _dbContext.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result<AuthenticationResponse>.Failure("User profile not found for refresh token.");
        }

        if (!user.IsActive)
        {
            return Result<AuthenticationResponse>.Failure("User account is inactive.");
        }

        var roles = await _identityService.GetRolesAsync(user.Id, cancellationToken);
        var accessTokenResult = _tokenService.GenerateAccessToken(user.Id, user.Email, user.FullName, roles, user.TeamId);
        var newRefreshTokenResult = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = TokenHashHelper.ComputeHash(newRefreshTokenResult.Token);

        refreshToken.Revoke(utcNow, user.Id.ToString(), newRefreshTokenHash);

        var replacementToken = RefreshToken.Create(user.Id, newRefreshTokenHash, newRefreshTokenResult.ExpiresOnUtc, utcNow);
        _dbContext.RefreshTokens.Add(replacementToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthenticationResponse(
            accessTokenResult.Token,
            newRefreshTokenResult.Token,
            accessTokenResult.ExpiresOnUtc,
            newRefreshTokenResult.ExpiresOnUtc,
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.TeamId);

        return Result<AuthenticationResponse>.Success(response);
    }
}
