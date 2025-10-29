using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Publishing;

public sealed class XPublisher : ISocialMediaPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XPublisher> _logger;

    public PlatformType Platform => PlatformType.X;

    public XPublisher(HttpClient httpClient, ILogger<XPublisher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SocialMediaPostResult> PublishAsync(SocialMediaPostRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing content to X (Twitter) for team {TeamId}", request.TeamId);

            // Build X/Twitter post content (limited to 280 characters)
            var tweetText = BuildTweetText(request);

            var postData = new
            {
                text = tweetText
            };

            var json = JsonSerializer.Serialize(postData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AccessToken);

            // X API v2 endpoint for creating tweets
            var response = await _httpClient.PostAsync("https://api.twitter.com/2/tweets", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<XPostResponse>(responseContent);
                
                _logger.LogInformation("Successfully published to X. Tweet ID: {TweetId}", result?.Data?.Id);
                return new SocialMediaPostResult(true, result?.Data?.Id, null);
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("X publish failed with status {StatusCode}: {Error}", response.StatusCode, error);
            
            var isRetryable = IsRetryableError(response.StatusCode, error);
            return new SocialMediaPostResult(false, null, error, isRetryable);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while publishing to X for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while publishing to X for team {TeamId}", request.TeamId);
            return new SocialMediaPostResult(false, null, ex.Message, false);
        }
    }

    private static string BuildTweetText(SocialMediaPostRequest request)
    {
        const int MaxTweetLength = 280;
        
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(request.Title))
            parts.Add(request.Title);
            
        if (!string.IsNullOrWhiteSpace(request.Body))
            parts.Add(request.Body);
            
        if (!string.IsNullOrWhiteSpace(request.Caption))
            parts.Add(request.Caption);
            
        if (request.Hashtags?.Length > 0)
            parts.Add(string.Join(" ", request.Hashtags.Select(h => h.StartsWith("#") ? h : $"#{h}")));

        var fullText = string.Join("\n\n", parts);
        
        // Truncate if necessary
        if (fullText.Length > MaxTweetLength)
        {
            fullText = fullText.Substring(0, MaxTweetLength - 3) + "...";
        }
        
        return fullText;
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
            _ => error.Contains("rate_limit_exceeded") || error.Contains("over_capacity")
        };
    }

    private sealed class XPostResponse
    {
        public XPostData? Data { get; set; }
    }

    private sealed class XPostData
    {
        public string? Id { get; set; }
        public string? Text { get; set; }
    }
}