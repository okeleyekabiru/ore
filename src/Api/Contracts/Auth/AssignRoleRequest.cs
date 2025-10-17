using System;

namespace Ore.Api.Contracts.Auth;

public sealed record AssignRoleRequest(Guid UserId, string Role);
