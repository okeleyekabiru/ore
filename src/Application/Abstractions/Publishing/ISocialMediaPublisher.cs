using Ore.Domain.Enums;

namespace Ore.Application.Abstractions.Publishing;

public interface ISocialMediaPublisher
{
    PlatformType Platform { get; }
    Task<SocialMediaPostResult> PublishAsync(SocialMediaPostRequest request, CancellationToken cancellationToken = default);
}

public sealed record SocialMediaPostRequest(
    string Title,
    string Body,
    string? Caption,
    string[]? Hashtags,
    string[]? ImageUrls,
    Guid TeamId,
    string AccessToken);

public sealed record SocialMediaPostResult(
    bool IsSuccess,
    string? ExternalPostId,
    string? ErrorMessage,
    bool IsRetryable = false);