using Ore.Domain.Common;

namespace Ore.Domain.Events;

public sealed class ContentGeneratedEvent : IDomainEvent
{
    public ContentGeneratedEvent(Guid contentId, Guid teamId, Guid requestedBy)
    {
        ContentId = contentId;
        TeamId = teamId;
        RequestedBy = requestedBy;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid ContentId { get; }
    public Guid TeamId { get; }
    public Guid RequestedBy { get; }
    public DateTime OccurredOnUtc { get; }
}
