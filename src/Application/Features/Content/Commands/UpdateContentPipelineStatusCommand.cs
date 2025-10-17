using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Application.Features.Content.Commands;
using Ore.Application.Features.Scheduling.Commands;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Content.Commands;

public sealed record UpdateContentPipelineStatusCommand(
    Guid ContentId,
    ContentStatus TargetStatus,
    Guid ActorId,
    DateTime? ScheduledOnUtc,
    string? Reason,
    PlatformType? Platform) : IRequest<Result<Guid>>;

public sealed class UpdateContentPipelineStatusCommandHandler : IRequestHandler<UpdateContentPipelineStatusCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public UpdateContentPipelineStatusCommandHandler(IApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(UpdateContentPipelineStatusCommand request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.ContentItems
            .Include(item => item.Distributions)
            .FirstOrDefaultAsync(item => item.Id == request.ContentId, cancellationToken);

        if (content is null)
        {
            return Result<Guid>.Failure("Content item was not found.");
        }

        if (content.Status == request.TargetStatus)
        {
            return Result<Guid>.Success(content.Id);
        }

        switch (request.TargetStatus)
        {
            case ContentStatus.Draft:
                content.ResetToDraft();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(content.Id);

            case ContentStatus.Generated:
                content.MarkGenerated();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(content.Id);

            case ContentStatus.PendingApproval:
                return await ForwardAsync(new SubmitContentForApprovalCommand(content.Id, request.ActorId), cancellationToken, content.Id);

            case ContentStatus.Approved:
                return await ForwardAsync(new ApproveContentCommand(content.Id, request.ActorId, request.Reason), cancellationToken, content.Id);

            case ContentStatus.Rejected:
            {
                var rejectionReason = string.IsNullOrWhiteSpace(request.Reason)
                    ? "Marked for revisions via content pipeline."
                    : request.Reason!;

                return await ForwardAsync(new RejectContentCommand(content.Id, request.ActorId, rejectionReason), cancellationToken, content.Id);
            }

            case ContentStatus.Scheduled:
            {
                if (request.ScheduledOnUtc is null)
                {
                    return Result<Guid>.Failure("scheduledOnUtc must be provided when moving content to Scheduled.");
                }

                var platform = request.Platform ?? GuessPlatform(content);
                var scheduleCommand = new ScheduleContentCommand(
                    content.Id,
                    platform,
                    request.ScheduledOnUtc.Value,
                    RetryInterval: null,
                    MaxRetryCount: 3);

                return await ForwardAsync(scheduleCommand, cancellationToken, content.Id);
            }

            case ContentStatus.Published:
                content.MarkPublished();
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(content.Id);

            default:
                return Result<Guid>.Failure("Unsupported pipeline status transition requested.");
        }
    }

    private async Task<Result<Guid>> ForwardAsync(IRequest<Result<Guid>> command, CancellationToken cancellationToken, Guid fallbackId)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.Succeeded)
        {
            var value = result.Value;
            return Result<Guid>.Success(value != Guid.Empty ? value : fallbackId);
        }

        return Result<Guid>.Failure(result.Errors.ToArray());
    }

    private static PlatformType GuessPlatform(ContentItem content)
    {
        var latest = content.Distributions
            .OrderByDescending(d => d.Window.PublishOnUtc)
            .FirstOrDefault();

        return latest?.Platform ?? PlatformType.LinkedIn;
    }
}
