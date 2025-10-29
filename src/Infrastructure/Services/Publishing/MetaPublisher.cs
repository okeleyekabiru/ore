using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Publishing;

public sealed class MetaPublisher : ISocialMediaPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetaPublisher> _logger;

    public PlatformType Platform => PlatformType.Meta;

    public MetaPublisher(HttpClient httpClient, ILogger<MetaPublisher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SocialMediaPostResult> PublishAsync(SocialMediaPostRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing content to Meta for team {TeamId}", request.TeamId);

            // Combine title, body, caption, and hashtags for Meta post
            var message = BuildMetaMessage(request);

            var postData = new
            {
                message = message,
                access_token = request.AccessToken
            };

            var json = JsonSerializer.Serialize(postData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Meta Graph API endpoint for posting to a page
            var response = await _httpClient.PostAsync("https://graph.facebook.com/v18.0/me/feed", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<MetaPostResponse>(responseContent);
                
                _logger.LogInformation("Successfully published to Meta. Post ID: {PostId}", result?.Id);
                return new SocialMediaPostResult(true, result?.Id, null);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Meta publish failed with status {StatusCode}: {Error}", response.StatusCode, error);
            
            // Check if error is retryable (rate limits, temporary failures)
            var isRetryable = IsRetryableError(response.StatusCode, error);
            return new SocialMediaPostResult(false, null, error, isRetryable);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while publishing to Meta for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing to Meta for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, false);
        }
    }

    private static string BuildMetaMessage(SocialMediaPostRequest request)
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
        // Rate limiting, server errors, and temporary issues are retryable
        return statusCode switch
        {
            System.Net.HttpStatusCode.TooManyRequests => true,
            System.Net.HttpStatusCode.InternalServerError => true,
            System.Net.HttpStatusCode.BadGateway => true,
            System.Net.HttpStatusCode.ServiceUnavailable => true,
            System.Net.HttpStatusCode.GatewayTimeout => true,
            _ => error.Contains("temporarily_unavailable") || error.Contains("rate_limit")
        };
    }

    private sealed class MetaPostResponse
    {
        public string? Id { get; set; }
    }
}