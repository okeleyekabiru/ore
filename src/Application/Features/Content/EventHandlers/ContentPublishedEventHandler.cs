using MediatR;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Messaging;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;

namespace Ore.Application.Features.Content.EventHandlers;

public sealed class ContentPublishedEventHandler : INotificationHandler<ContentPublishedEvent>
{
    private readonly INotificationHubService _notificationHubService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ContentPublishedEventHandler> _logger;

    public ContentPublishedEventHandler(
        INotificationHubService notificationHubService,
        INotificationService notificationService,
        ILogger<ContentPublishedEventHandler> logger)
    {
        _notificationHubService = notificationHubService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ContentPublishedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var platformName = notification.Platform.ToString();
            var message = $"Your content has been successfully published to {platformName}!";
            var title = $"Published to {platformName} 🚀";

            // Send persistent notification
            await _notificationService.DispatchAsync(
                Guid.Empty, // We'll need to get the content owner from the distribution
                NotificationType.Published,
                message,
                cancellationToken);

            // Send real-time notification
            var realTimeData = new
            {
                ContentDistributionId = notification.ContentDistributionId,
                Platform = platformName,
                Message = message,
                Type = "success",
                Timestamp = DateTime.UtcNow
            };

            // For now, broadcast to all users - in a real scenario, we'd get the content owner
            await _notificationHubService.SendToAllAsync(
                "ContentPublished",
                realTimeData,
                cancellationToken);

            _logger.LogInformation("Content published notification sent for distribution {DistributionId} on platform {Platform}",
                notification.ContentDistributionId, notification.Platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle content published event for distribution {DistributionId}",
                notification.ContentDistributionId);
        }
    }
}