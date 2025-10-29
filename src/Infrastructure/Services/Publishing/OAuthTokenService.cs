using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Publishing;

public sealed class OAuthTokenService : IOAuthTokenService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<OAuthTokenService> _logger;

    public OAuthTokenService(IApplicationDbContext dbContext, ILogger<OAuthTokenService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string?> GetValidAccessTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default)
    {
        var account = await GetAccountAsync(teamId, platform, cancellationToken);
        
        if (account is null || !account.IsActive)
        {
            _logger.LogWarning("No active social media account found for team {TeamId} on platform {Platform}", teamId, platform);
            return null;
        }

        // Check if token is expired
        if (account.IsExpired)
        {
            _logger.LogInformation("Access token expired for team {TeamId} on platform {Platform}, attempting refresh", teamId, platform);
            
            if (await RefreshTokenAsync(teamId, platform, cancellationToken))
            {
                // Reload the account to get the refreshed token
                account = await GetAccountAsync(teamId, platform, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Failed to refresh token for team {TeamId} on platform {Platform}", teamId, platform);
                return null;
            }
        }

        // Mark token as used
        account?.MarkUsed();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account?.AccessToken;
    }

    public async Task<bool> RefreshTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default)
    {
        var account = await GetAccountAsync(teamId, platform, cancellationToken);
        
        if (account is null || !account.NeedsRefresh)
        {
            _logger.LogDebug("Account for team {TeamId} on platform {Platform} does not need refresh", teamId, platform);
            return false;
        }

        try
        {
            // TODO: Implement actual OAuth refresh logic for each platform
            // This would involve calling the platform's OAuth refresh endpoint
            var refreshResult = await RefreshTokenForPlatformAsync(account.RefreshToken!, platform, cancellationToken);
            
            if (refreshResult.IsSuccess && refreshResult.AccessToken is not null)
            {
                account.UpdateTokens(refreshResult.AccessToken, refreshResult.RefreshToken, refreshResult.ExpiresAt);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully refreshed token for team {TeamId} on platform {Platform}", teamId, platform);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to refresh token for team {TeamId} on platform {Platform}: {Error}", 
                    teamId, platform, refreshResult.ErrorMessage);
                
                // If refresh fails, deactivate the account
                account.Deactivate();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while refreshing token for team {TeamId} on platform {Platform}", teamId, platform);
            return false;
        }
    }

    public async Task StoreTokensAsync(Guid teamId, PlatformType platform, string accountName, string accessToken, string? refreshToken = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var existingAccount = await GetAccountAsync(teamId, platform, cancellationToken);
        
        if (existingAccount is not null)
        {
            // Update existing account
            existingAccount.UpdateTokens(accessToken, refreshToken, expiresAt);
            existingAccount.Activate();
            _logger.LogInformation("Updated tokens for existing account {AccountName} (team {TeamId} on platform {Platform})", 
                accountName, teamId, platform);
        }
        else
        {
            // Create new account
            var newAccount = new SocialMediaAccount(teamId, platform, accountName, accessToken, refreshToken, expiresAt);
            _dbContext.SocialMediaAccounts.Add(newAccount);
            _logger.LogInformation("Created new social media account {AccountName} for team {TeamId} on platform {Platform}", 
                accountName, teamId, platform);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeTokensAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default)
    {
        var account = await GetAccountAsync(teamId, platform, cancellationToken);
        
        if (account is not null)
        {
            account.Deactivate();
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Revoked tokens for team {TeamId} on platform {Platform}", teamId, platform);
        }
    }

    public async Task<bool> HasValidTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default)
    {
        var account = await GetAccountAsync(teamId, platform, cancellationToken);
        return account is not null && account.IsActive && !account.IsExpired;
    }

    private async Task<SocialMediaAccount?> GetAccountAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken)
    {
        return await _dbContext.SocialMediaAccounts
            .FirstOrDefaultAsync(a => a.TeamId == teamId && a.Platform == platform && a.IsActive, cancellationToken);
    }

    private async Task<TokenRefreshResult> RefreshTokenForPlatformAsync(string refreshToken, PlatformType platform, CancellationToken cancellationToken)
    {
        // TODO: Implement actual refresh logic for each platform
        // For now, return a placeholder result
        _logger.LogWarning("Token refresh not yet implemented for platform {Platform}", platform);
        
        return platform switch
        {
            PlatformType.Meta => await RefreshMetaTokenAsync(refreshToken, cancellationToken),
            PlatformType.LinkedIn => await RefreshLinkedInTokenAsync(refreshToken, cancellationToken),
            PlatformType.X => await RefreshXTokenAsync(refreshToken, cancellationToken),
            _ => new TokenRefreshResult(false, null, null, null, "Platform not supported for token refresh")
        };
    }

    private async Task<TokenRefreshResult> RefreshMetaTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // TODO: Implement Meta OAuth refresh
        await Task.Delay(1, cancellationToken); // Placeholder
        return new TokenRefreshResult(false, null, null, null, "Meta token refresh not implemented");
    }

    private async Task<TokenRefreshResult> RefreshLinkedInTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // TODO: Implement LinkedIn OAuth refresh  
        await Task.Delay(1, cancellationToken); // Placeholder
        return new TokenRefreshResult(false, null, null, null, "LinkedIn token refresh not implemented");
    }

    private async Task<TokenRefreshResult> RefreshXTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // TODO: Implement X OAuth refresh
        await Task.Delay(1, cancellationToken); // Placeholder
        return new TokenRefreshResult(false, null, null, null, "X token refresh not implemented");
    }

    private sealed record TokenRefreshResult(bool IsSuccess, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt, string? ErrorMessage);
}