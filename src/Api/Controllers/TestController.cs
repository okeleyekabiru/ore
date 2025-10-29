using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Domain.Enums;
using Ore.Domain.Events;
using MediatR;

namespace Ore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationHubService _notificationHubService;

    public TestController(IMediator mediator, INotificationHubService notificationHubService)
    {
        _mediator = mediator;
        _notificationHubService = notificationHubService;
    }

    [HttpPost("trigger-approval-event")]
    public async Task<IActionResult> TriggerApprovalEvent([FromBody] TriggerApprovalRequest request)
    {
        var approvalEvent = new ContentApprovalEvent(
            request.ContentId,
            request.ApproverId,
            request.Status);

        await _mediator.Publish(approvalEvent);

        return Ok(new { message = "Content approval event triggered", contentId = request.ContentId, status = request.Status });
    }

    [HttpPost("trigger-published-event")]
    public async Task<IActionResult> TriggerPublishedEvent([FromBody] TriggerPublishedRequest request)
    {
        var publishedEvent = new ContentPublishedEvent(
            request.ContentDistributionId,
            request.Platform);

        await _mediator.Publish(publishedEvent);

        return Ok(new { message = "Content published event triggered", distributionId = request.ContentDistributionId, platform = request.Platform });
    }

    [HttpPost("trigger-scheduled-event")]
    public async Task<IActionResult> TriggerScheduledEvent([FromBody] TriggerScheduledRequest request)
    {
        var scheduledEvent = new ContentScheduledEvent(
            request.ContentDistributionId,
            request.Platform,
            request.PublishOnUtc);

        await _mediator.Publish(scheduledEvent);

        return Ok(new { message = "Content scheduled event triggered", distributionId = request.ContentDistributionId, platform = request.Platform, publishOn = request.PublishOnUtc });
    }

    [HttpPost("send-direct-notification")]
    public async Task<IActionResult> SendDirectNotification([FromBody] DirectNotificationRequest request)
    {
        await _notificationHubService.SendToAllAsync(request.Method, request.Data);

        return Ok(new { message = "Direct notification sent", method = request.Method });
    }
}

public record TriggerApprovalRequest(Guid ContentId, Guid ApproverId, ApprovalStatus Status);
public record TriggerPublishedRequest(Guid ContentDistributionId, PlatformType Platform);
public record TriggerScheduledRequest(Guid ContentDistributionId, PlatformType Platform, DateTime PublishOnUtc);
public record DirectNotificationRequest(string Method, object Data);