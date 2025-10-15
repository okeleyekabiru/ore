using System.Security.Cryptography;
using System.Text;

namespace Ore.Application.Features.Users.Commands;

internal static class TokenHashHelper
{
    public static string ComputeHash(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashedBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashedBytes);
    }
}
