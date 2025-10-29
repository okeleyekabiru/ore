using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Publishing;

public sealed class SocialMediaPublisherFactory : ISocialMediaPublisherFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SocialMediaPublisherFactory> _logger;

    public SocialMediaPublisherFactory(IServiceProvider serviceProvider, ILogger<SocialMediaPublisherFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public ISocialMediaPublisher GetPublisher(PlatformType platform)
    {
        ISocialMediaPublisher publisher = platform switch
        {
            PlatformType.Meta => _serviceProvider.GetRequiredService<MetaPublisher>(),
            PlatformType.LinkedIn => _serviceProvider.GetRequiredService<LinkedInPublisher>(),
            PlatformType.X => _serviceProvider.GetRequiredService<XPublisher>(),
            _ => throw new NotSupportedException($"Publishing to {platform} is not yet supported.")
        };

        _logger.LogDebug("Retrieved publisher for platform {Platform}: {PublisherType}", platform, publisher.GetType().Name);
        return publisher;
    }

    public IEnumerable<ISocialMediaPublisher> GetAllPublishers()
    {
        yield return _serviceProvider.GetRequiredService<MetaPublisher>();
        yield return _serviceProvider.GetRequiredService<LinkedInPublisher>();
        yield return _serviceProvider.GetRequiredService<XPublisher>();
    }
}