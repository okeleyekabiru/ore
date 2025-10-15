namespace Ore.Application.Abstractions.Identity;

public interface IJwtTokenService
{
    JwtTokenResult GenerateAccessToken(Guid userId, string email, string fullName, IEnumerable<string> roles, Guid? teamId = null);
    RefreshTokenResult GenerateRefreshToken();
}
