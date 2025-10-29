using Ore.Domain.Common;

namespace Ore.Domain.Entities;

public sealed class AuditLog : AuditableEntity, IAggregateRoot
{
    private AuditLog()
    {
    }

    public AuditLog(string actor, string action, string entity, string entityId, string metadata, string? userId = null, string? ipAddress = null)
    {
        Actor = actor;
        Action = action;
        Entity = entity;
        EntityId = entityId;
        Metadata = metadata;
        UserId = userId;
        IPAddress = ipAddress;
    }

    public string Actor { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Entity { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Metadata { get; private set; } = string.Empty;
    public string? UserId { get; private set; }
    public string? IPAddress { get; private set; }
}
