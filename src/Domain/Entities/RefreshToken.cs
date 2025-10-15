using System;
using Ore.Domain.Common;

namespace Ore.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken()
    {
    }

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresOnUtc, DateTime createdOnUtc, string createdBy)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresOnUtc = expiresOnUtc;
        CreatedOnUtc = createdOnUtc;
        CreatedBy = createdBy;
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; private set; }
    public DateTime? RevokedOnUtc { get; private set; }
    public string? RevokedBy { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresOnUtc;
    public bool IsRevoked => RevokedOnUtc is not null;
    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresOnUtc, DateTime utcNow)
    {
        return new RefreshToken(userId, tokenHash, expiresOnUtc, utcNow, userId.ToString());
    }

    public void Revoke(DateTime utcNow, string? revokedBy, string? replacedByHash = null)
    {
        RevokedOnUtc = utcNow;
        RevokedBy = revokedBy;
        ReplacedByTokenHash = replacedByHash;
    }
}
