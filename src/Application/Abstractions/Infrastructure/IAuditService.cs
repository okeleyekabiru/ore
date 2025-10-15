using System.Threading;
using System.Threading.Tasks;

namespace Ore.Application.Abstractions.Infrastructure;

public interface IAuditService
{
    Task LogAsync(string actor, string action, string entity, string entityId, string metadata, CancellationToken cancellationToken = default);
}
