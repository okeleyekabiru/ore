using System;
using System.Linq;
using Ore.Application.Features.Content.Queries;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Content.Common;

public static class ContentPipelineMapper
{
    public static ContentPipelineItemDto MapToDto(ContentItem item)
    {
        var updatedOn = item.ModifiedOnUtc ?? item.CreatedOnUtc;

        var upcomingDistribution = item.Distributions
            .OrderBy(d => d.Window.PublishOnUtc)
            .FirstOrDefault(d => d.Window.PublishOnUtc >= DateTime.UtcNow);

        var lastDistribution = upcomingDistribution ?? item.Distributions
            .OrderByDescending(d => d.Window.PublishOnUtc)
            .FirstOrDefault();

        var channel = lastDistribution is null
            ? new ContentPipelineChannelDto("unassigned", "Unassigned")
            : new ContentPipelineChannelDto(GetPlatformIdentifier(lastDistribution.Platform), GetPlatformDisplayName(lastDistribution.Platform));

        var scheduledOn = lastDistribution?.Window.PublishOnUtc;
        var dueOn = item.Status is ContentStatus.Scheduled or ContentStatus.Published ? scheduledOn : null;

        var ownerName = item.Author switch
        {
            null => null,
            _ => string.IsNullOrWhiteSpace(item.Author.FullName) ? item.Author.Email : item.Author.FullName,
        };

        var owner = new ContentPipelineOwnerDto(item.AuthorId, ownerName);

        return new ContentPipelineItemDto(
            item.Id,
            item.TeamId,
            item.Title,
            item.Status.ToString(),
            channel,
            owner,
            updatedOn,
            dueOn,
            scheduledOn);
    }

    private static string GetPlatformIdentifier(PlatformType platform) => platform.ToString();

    private static string GetPlatformDisplayName(PlatformType platform) => platform switch
    {
        PlatformType.Meta => "Meta",
        PlatformType.X => "X (Twitter)",
        PlatformType.LinkedIn => "LinkedIn",
        PlatformType.Instagram => "Instagram",
        PlatformType.TikTok => "TikTok",
        _ => platform.ToString()
    };
}