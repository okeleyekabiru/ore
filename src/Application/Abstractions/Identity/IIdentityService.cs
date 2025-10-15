using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ore.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<Guid> CreateUserAsync(string email, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default);
    Task AssignToTeamAsync(Guid userId, Guid teamId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
}
