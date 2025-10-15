using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Abstractions.Scheduling;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Features.Scheduling.Commands;

public sealed record ScheduleContentCommand(Guid ContentId, PlatformType Platform, DateTime PublishOnUtc, TimeSpan? RetryInterval, int MaxRetryCount)
    : IRequest<Result<Guid>>;

public sealed class ScheduleContentCommandHandler : IRequestHandler<ScheduleContentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISchedulingService _schedulingService;

    public ScheduleContentCommandHandler(IApplicationDbContext dbContext, ISchedulingService schedulingService)
    {
        _dbContext = dbContext;
        _schedulingService = schedulingService;
    }

    public async Task<Result<Guid>> Handle(ScheduleContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.ContentItems
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content is null)
        {
            return Result<Guid>.Failure("Content not found");
        }

        var window = PublishingWindow.Create(request.PublishOnUtc, request.RetryInterval, request.MaxRetryCount);
        var distribution = new ContentDistribution(content.Id, request.Platform, window);

    content.Schedule(distribution);
    content.AddDomainEvent(new ContentScheduledEvent(distribution.Id, request.Platform, window.PublishOnUtc));

        _dbContext.ContentDistributions.Add(distribution);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _schedulingService.ScheduleAsync(distribution, cancellationToken);

        return Result<Guid>.Success(distribution.Id);
    }
}
