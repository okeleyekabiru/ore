using Ore.Domain.Enums;

namespace Ore.Application.Abstractions.Publishing;

public interface ISocialMediaPublisherFactory
{
    ISocialMediaPublisher GetPublisher(PlatformType platform);
    IEnumerable<ISocialMediaPublisher> GetAllPublishers();
}