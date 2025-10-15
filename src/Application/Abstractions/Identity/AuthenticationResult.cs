namespace Ore.Application.Abstractions.Identity;

public sealed record AuthenticationResult(Guid UserId, string Email, IReadOnlyCollection<string> Roles);
