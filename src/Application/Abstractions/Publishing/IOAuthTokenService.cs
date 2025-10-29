using Ore.Domain.Enums;

namespace Ore.Application.Abstractions.Publishing;

public interface IOAuthTokenService
{
    Task<string?> GetValidAccessTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default);
    Task<bool> RefreshTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default);
    Task StoreTokensAsync(Guid teamId, PlatformType platform, string accountName, string accessToken, string? refreshToken = null, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task RevokeTokensAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default);
    Task<bool> HasValidTokenAsync(Guid teamId, PlatformType platform, CancellationToken cancellationToken = default);
}