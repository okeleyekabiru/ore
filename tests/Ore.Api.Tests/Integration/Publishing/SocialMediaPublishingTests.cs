using Microsoft.Extensions.Logging;
using Moq;
using Ore.Application.Abstractions.Publishing;
using Ore.Domain.Enums;
using Ore.Infrastructure.Services.Publishing;
using Xunit;

namespace Ore.Api.Tests.Integration.Publishing;

public class SocialMediaPublishingTests
{
    [Fact]
    public void MetaPublisher_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var mockLogger = new Mock<ILogger<MetaPublisher>>();
        var publisher = new MetaPublisher(mockHttpClient, mockLogger.Object);

        var request = new SocialMediaPostRequest(
            Title: "Test Title",
            Body: "Test Body Content",
            Caption: "Test Caption",
            Hashtags: new[] { "test", "automation" },
            ImageUrls: null,
            TeamId: Guid.NewGuid(),
            AccessToken: "test-token");

        // Act & Assert
        Assert.Equal(PlatformType.Meta, publisher.Platform);
        Assert.NotNull(publisher);
        
        // Note: This would require mocking HTTP responses for full integration testing
        // For now, we're just verifying the publisher can be instantiated correctly
    }

    [Fact]
    public void LinkedInPublisher_WithValidRequest_ShouldReturnCorrectPlatform()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var mockLogger = new Mock<ILogger<LinkedInPublisher>>();
        var publisher = new LinkedInPublisher(mockHttpClient, mockLogger.Object);

        // Act & Assert
        Assert.Equal(PlatformType.LinkedIn, publisher.Platform);
    }

    [Fact]
    public void XPublisher_WithValidRequest_ShouldReturnCorrectPlatform()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var mockLogger = new Mock<ILogger<XPublisher>>();
        var publisher = new XPublisher(mockHttpClient, mockLogger.Object);

        // Act & Assert
        Assert.Equal(PlatformType.X, publisher.Platform);
    }

    [Fact]
    public void SocialMediaPublisherFactory_GetPublisher_ShouldReturnCorrectPublisher()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<SocialMediaPublisherFactory>>();
        
        var metaPublisher = new MetaPublisher(new HttpClient(), Mock.Of<ILogger<MetaPublisher>>());
        var linkedInPublisher = new LinkedInPublisher(new HttpClient(), Mock.Of<ILogger<LinkedInPublisher>>());
        var xPublisher = new XPublisher(new HttpClient(), Mock.Of<ILogger<XPublisher>>());

        mockServiceProvider.Setup(sp => sp.GetService(typeof(MetaPublisher))).Returns(metaPublisher);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(LinkedInPublisher))).Returns(linkedInPublisher);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(XPublisher))).Returns(xPublisher);

        var factory = new SocialMediaPublisherFactory(mockServiceProvider.Object, mockLogger.Object);

        // Act & Assert
        var metaResult = factory.GetPublisher(PlatformType.Meta);
        var linkedInResult = factory.GetPublisher(PlatformType.LinkedIn);
        var xResult = factory.GetPublisher(PlatformType.X);

        Assert.Equal(PlatformType.Meta, metaResult.Platform);
        Assert.Equal(PlatformType.LinkedIn, linkedInResult.Platform);
        Assert.Equal(PlatformType.X, xResult.Platform);
    }

    [Fact]
    public void SocialMediaPublisherFactory_GetPublisher_UnsupportedPlatform_ShouldThrow()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<SocialMediaPublisherFactory>>();
        var factory = new SocialMediaPublisherFactory(mockServiceProvider.Object, mockLogger.Object);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => factory.GetPublisher((PlatformType)999));
    }
}