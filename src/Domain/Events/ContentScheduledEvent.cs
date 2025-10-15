using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Events;

public sealed class ContentScheduledEvent : IDomainEvent
{
    public ContentScheduledEvent(Guid contentDistributionId, PlatformType platform, DateTime publishOnUtc)
    {
        ContentDistributionId = contentDistributionId;
        Platform = platform;
        PublishOnUtc = publishOnUtc;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid ContentDistributionId { get; }
    public PlatformType Platform { get; }
    public DateTime PublishOnUtc { get; }
    public DateTime OccurredOnUtc { get; }
}
