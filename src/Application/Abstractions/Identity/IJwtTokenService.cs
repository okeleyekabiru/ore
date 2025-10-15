namespace Ore.Application.Abstractions.Identity;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string fullName, IEnumerable<string> roles, Guid? teamId = null);
}
