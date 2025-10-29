using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class SocialMediaAccount : AuditableEntity
{
    private SocialMediaAccount()
    {
    }

    public SocialMediaAccount(Guid teamId, PlatformType platform, string accountName, string accessToken, string? refreshToken, DateTime? expiresAt)
    {
        TeamId = teamId;
        Platform = platform;
        AccountName = accountName;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public Guid TeamId { get; private set; }
    public PlatformType Platform { get; private set; }
    public string AccountName { get; private set; } = string.Empty;
    public string AccessToken { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    public void UpdateTokens(string accessToken, string? refreshToken, DateTime? expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    public void MarkUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    public bool NeedsRefresh => IsExpired && !string.IsNullOrEmpty(RefreshToken);
}