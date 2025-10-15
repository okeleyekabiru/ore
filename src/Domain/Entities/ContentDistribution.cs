using System;
using Ore.Domain.Common;
using Ore.Domain.Enums;
using Ore.Domain.ValueObjects;

namespace Ore.Domain.Entities;

public sealed class ContentDistribution : BaseEntity
{
    private ContentDistribution()
    {
    }

    public ContentDistribution(Guid contentItemId, PlatformType platform, PublishingWindow window)
    {
        ContentItemId = contentItemId;
        Platform = platform;
        Window = window;
    }

    public Guid ContentItemId { get; private set; }
    public ContentItem? ContentItem { get; private set; }
    public PlatformType Platform { get; private set; }
    public PublishingWindow Window { get; private set; } = PublishingWindow.Create(DateTime.UtcNow.AddHours(1));
    public string? ExternalPostId { get; private set; }
    public DateTime? PublishedOnUtc { get; private set; }
    public string? FailureReason { get; private set; }

    public void MarkPublished(string externalPostId)
    {
        ExternalPostId = externalPostId;
        PublishedOnUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        FailureReason = reason;
    }
}
