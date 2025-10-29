using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Publishing;

public sealed class LinkedInPublisher : ISocialMediaPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkedInPublisher> _logger;

    public PlatformType Platform => PlatformType.LinkedIn;

    public LinkedInPublisher(HttpClient httpClient, ILogger<LinkedInPublisher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SocialMediaPostResult> PublishAsync(SocialMediaPostRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing content to LinkedIn for team {TeamId}", request.TeamId);

            // Build LinkedIn post content
            var commentary = BuildLinkedInCommentary(request);

            var postData = new
            {
                author = "urn:li:person:PERSON_ID", // This would need to be dynamically set based on the authenticated user
                lifecycleState = "PUBLISHED",
                specificContent = new
                {
                    comLinkedinUgcShareContent = new
                    {
                        shareCommentary = new
                        {
                            text = commentary
                        },
                        shareMediaCategory = "NONE"
                    }
                },
                visibility = new
                {
                    comLinkedinUgcMemberNetworkVisibility = "PUBLIC"
                }
            };

            var json = JsonSerializer.Serialize(postData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AccessToken);

            // LinkedIn API v2 endpoint for creating posts
            var response = await _httpClient.PostAsync("https://api.linkedin.com/v2/ugcPosts", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<LinkedInPostResponse>(responseContent);
                
                _logger.LogInformation("Successfully published to LinkedIn. Post ID: {PostId}", result?.Id);
                return new SocialMediaPostResult(true, result?.Id, null);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("LinkedIn publish failed with status {StatusCode}: {Error}", response.StatusCode, error);
            
            var isRetryable = IsRetryableError(response.StatusCode, error);
            return new SocialMediaPostResult(false, null, error, isRetryable);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while publishing to LinkedIn for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing to LinkedIn for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, false);
        }
    }

    private static string BuildLinkedInCommentary(SocialMediaPostRequest request)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(request.Title))
            parts.Add(request.Title);
            
        if (!string.IsNullOrWhiteSpace(request.Body))
            parts.Add(request.Body);
            
        if (!string.IsNullOrWhiteSpace(request.Caption))
            parts.Add(request.Caption);
            
        if (request.Hashtags?.Length > 0)
            parts.Add(string.Join(" ", request.Hashtags.Select(h => h.StartsWith("#") ? h : $"#{h}")));

        return string.Join("\n\n", parts);
    }

    private static bool IsRetryableError(System.Net.HttpStatusCode statusCode, string error)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.TooManyRequests => true,
            System.Net.HttpStatusCode.InternalServerError => true,
            System.Net.HttpStatusCode.BadGateway => true,
            System.Net.HttpStatusCode.ServiceUnavailable => true,
            System.Net.HttpStatusCode.GatewayTimeout => true,
            _ => error.Contains("THROTTLE_LIMIT_EXCEEDED") || error.Contains("INTERNAL_SERVER_ERROR")
        };
    }

    private sealed class LinkedInPostResponse
    {
        public string? Id { get; set; }
    }
}