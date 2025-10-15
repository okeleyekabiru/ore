using System;
using System.Text;

namespace Ore.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;

    public byte[] GetSigningKeyBytes()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new InvalidOperationException("Jwt:Key is not configured.");
        }

        try
        {
            return Convert.FromBase64String(Key);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(Key);
        }
    }
}
