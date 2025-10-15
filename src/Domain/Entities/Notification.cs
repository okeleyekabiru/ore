using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class Notification : AuditableEntity, IAggregateRoot
{
    private Notification()
    {
    }

    public Notification(Guid recipientId, NotificationType type, string message)
    {
        RecipientId = recipientId;
        Type = type;
        Message = message;
    }

    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    public void MarkRead()
    {
        IsRead = true;
    }
}
