using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Scheduling;
using Ore.Domain.Entities;
using Quartz;

namespace Ore.Infrastructure.Services.Scheduling;

public sealed class QuartzSchedulingService : ISchedulingService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzSchedulingService> _logger;

    public QuartzSchedulingService(ISchedulerFactory schedulerFactory, ILogger<QuartzSchedulingService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task ScheduleAsync(ContentDistribution distribution, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);

        var job = JobBuilder.Create<ContentDistributionJob>()
            .WithIdentity(GetJobKey(distribution.Id))
            .UsingJobData(ContentDistributionJob.DistributionIdKey, distribution.Id.ToString())
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(GetTriggerKey(distribution.Id))
            .StartAt(DateTimeOffset.UtcNow >= distribution.Window.PublishOnUtc
                ? DateTimeOffset.UtcNow.AddSeconds(5)
                : new DateTimeOffset(distribution.Window.PublishOnUtc, TimeSpan.Zero))
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Scheduled distribution {DistributionId} at {PublishOn}.", distribution.Id, distribution.Window.PublishOnUtc);
    }

    public async Task CancelAsync(Guid distributionId, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);
        var jobKey = GetJobKey(distributionId);
        var cancelled = await scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        if (cancelled)
        {
            _logger.LogInformation("Cancelled schedule for distribution {DistributionId}.", distributionId);
        }
    }

    private static JobKey GetJobKey(Guid distributionId) => new(distributionId.ToString(), nameof(ContentDistribution));
    private static TriggerKey GetTriggerKey(Guid distributionId) => new(distributionId.ToString(), nameof(ContentDistribution));
}

public sealed class ContentDistributionJob : IJob
{
    public const string DistributionIdKey = "DistributionId";

    private readonly ILogger<ContentDistributionJob> _logger;

    public ContentDistributionJob(ILogger<ContentDistributionJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var distributionId = context.MergedJobDataMap.GetString(DistributionIdKey);
        _logger.LogInformation("Executing distribution job for {DistributionId} at {Timestamp}.", distributionId, DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
