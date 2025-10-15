using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Events;

public sealed class ContentPublishedEvent : IDomainEvent
{
    public ContentPublishedEvent(Guid contentDistributionId, PlatformType platform)
    {
        ContentDistributionId = contentDistributionId;
        Platform = platform;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid ContentDistributionId { get; }
    public PlatformType Platform { get; }
    public DateTime OccurredOnUtc { get; }
}
