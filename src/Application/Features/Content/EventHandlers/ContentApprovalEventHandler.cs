using MediatR;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Messaging;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;

namespace Ore.Application.Features.Content.EventHandlers;

public sealed class ContentApprovalEventHandler : INotificationHandler<ContentApprovalEvent>
{
    private readonly INotificationHubService _notificationHubService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ContentApprovalEventHandler> _logger;

    public ContentApprovalEventHandler(
        INotificationHubService notificationHubService,
        INotificationService notificationService,
        ILogger<ContentApprovalEventHandler> logger)
    {
        _notificationHubService = notificationHubService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ContentApprovalEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var message = notification.Status switch
            {
                ApprovalStatus.Approved => "Your content has been approved and is ready for publishing!",
                ApprovalStatus.Rejected => "Your content has been rejected. Please review the feedback and resubmit.",
                _ => "Your content approval status has been updated."
            };

            var notificationType = notification.Status switch
            {
                ApprovalStatus.Approved => NotificationType.ApprovalDecision,
                ApprovalStatus.Rejected => NotificationType.ApprovalDecision,
                _ => NotificationType.ApprovalDecision
            };

            // Send persistent notification
            await _notificationService.DispatchAsync(
                notification.ApproverId,
                notificationType,
                message,
                cancellationToken);

            // Send real-time notification
            var realTimeData = new
            {
                ContentId = notification.ContentId,
                Message = message,
                Type = notification.Status == ApprovalStatus.Approved ? "success" : "warning",
                Timestamp = DateTime.UtcNow,
                Status = notification.Status.ToString()
            };

            await _notificationHubService.SendToUserAsync(
                notification.ApproverId.ToString(),
                "ContentApprovalUpdate",
                realTimeData,
                cancellationToken);

            _logger.LogInformation("Content approval notification sent for content {ContentId} with status {Status}",
                notification.ContentId, notification.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle content approval event for content {ContentId}",
                notification.ContentId);
        }
    }
}