using MediatR;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Messaging;
using Ore.Domain.Enums;
using Ore.Domain.Events;

namespace Ore.Application.Features.Content.EventHandlers;

public sealed class ContentScheduledEventHandler : INotificationHandler<ContentScheduledEvent>
{
    private readonly INotificationHubService _notificationHubService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ContentScheduledEventHandler> _logger;

    public ContentScheduledEventHandler(
        INotificationHubService notificationHubService,
        INotificationService notificationService,
        ILogger<ContentScheduledEventHandler> logger)
    {
        _notificationHubService = notificationHubService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ContentScheduledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var platformName = notification.Platform.ToString();
            var message = $"Your content has been scheduled for publishing to {platformName} at {notification.PublishOnUtc:yyyy-MM-dd HH:mm} UTC";

            // Send persistent notification
            await _notificationService.DispatchAsync(
                Guid.Empty, // We'll need to get the content owner from the distribution
                NotificationType.Scheduled,
                message,
                cancellationToken);

            // Send real-time notification
            var realTimeData = new
            {
                ContentDistributionId = notification.ContentDistributionId,
                Platform = platformName,
                PublishOnUtc = notification.PublishOnUtc,
                Message = message,
                Type = "info",
                Timestamp = DateTime.UtcNow
            };

            // For now, broadcast to all users - in a real scenario, we'd get the content owner
            await _notificationHubService.SendToAllAsync(
                "ContentScheduled",
                realTimeData,
                cancellationToken);

            _logger.LogInformation("Content scheduled notification sent for distribution {DistributionId} on platform {Platform}",
                notification.ContentDistributionId, notification.Platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle content scheduled event for distribution {DistributionId}",
                notification.ContentDistributionId);
        }
    }
}