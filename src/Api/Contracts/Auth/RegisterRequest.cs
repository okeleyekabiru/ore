using Ore.Domain.Enums;

namespace Ore.Api.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    RoleType Role,
    string? TeamName,
    bool IsIndividual);
