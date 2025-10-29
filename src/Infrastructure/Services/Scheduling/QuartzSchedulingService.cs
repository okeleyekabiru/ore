using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Abstractions.Publishing;
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

    private readonly IApplicationDbContext _dbContext;
    private readonly ISocialMediaPublisherFactory _publisherFactory;
    private readonly IOAuthTokenService _oauthTokenService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ContentDistributionJob> _logger;

    public ContentDistributionJob(
        IApplicationDbContext dbContext,
        ISocialMediaPublisherFactory publisherFactory,
        IOAuthTokenService oauthTokenService,
        IAuditService auditService,
        ILogger<ContentDistributionJob> logger)
    {
        _dbContext = dbContext;
        _publisherFactory = publisherFactory;
        _oauthTokenService = oauthTokenService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var distributionIdString = context.MergedJobDataMap.GetString(DistributionIdKey);
        
        if (!Guid.TryParse(distributionIdString, out var distributionId))
        {
            _logger.LogError("Invalid distribution ID: {DistributionId}", distributionIdString);
            return;
        }

        _logger.LogInformation("Executing distribution job for {DistributionId} at {Timestamp}", distributionId, DateTime.UtcNow);

        try
        {
            await PublishContentAsync(distributionId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute distribution job for {DistributionId}", distributionId);
            
            // Schedule retry if within retry limits
            await HandleRetryAsync(distributionId, ex.Message, context);
        }
    }

    private async Task PublishContentAsync(Guid distributionId, CancellationToken cancellationToken)
    {
        var distribution = await _dbContext.ContentDistributions
            .Include(d => d.ContentItem)
            .FirstOrDefaultAsync(d => d.Id == distributionId, cancellationToken);

        if (distribution?.ContentItem is null)
        {
            _logger.LogWarning("Distribution {DistributionId} or its content not found", distributionId);
            return;
        }

        if (distribution.PublishedOnUtc.HasValue)
        {
            _logger.LogInformation("Distribution {DistributionId} already published on {PublishedOn}", 
                distributionId, distribution.PublishedOnUtc);
            return;
        }

        // Get OAuth token for the team and platform
        var accessToken = await _oauthTokenService.GetValidAccessTokenAsync(distribution.ContentItem.TeamId, distribution.Platform, cancellationToken);
        
        if (string.IsNullOrEmpty(accessToken))
        {
            var errorMessage = $"No valid access token found for team {distribution.ContentItem.TeamId} on platform {distribution.Platform}";
            _logger.LogWarning(errorMessage);
            distribution.MarkFailed(errorMessage);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var publisher = _publisherFactory.GetPublisher(distribution.Platform);
        var request = new SocialMediaPostRequest(
            distribution.ContentItem.Title,
            distribution.ContentItem.Body,
            distribution.ContentItem.Caption,
            distribution.ContentItem.Hashtags?.ToArray(),
            null, // Image URLs - TODO: implement image handling
            distribution.ContentItem.TeamId,
            accessToken);

        var result = await publisher.PublishAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            distribution.MarkPublished(result.ExternalPostId ?? "unknown");
            distribution.ContentItem.MarkPublished();
            _logger.LogInformation("Successfully published distribution {DistributionId} to {Platform}. External ID: {ExternalId}", 
                distributionId, distribution.Platform, result.ExternalPostId);
                
            // Audit successful publish
            var successMetadata = JsonSerializer.Serialize(new
            {
                DistributionId = distributionId,
                Platform = distribution.Platform.ToString(),
                ExternalPostId = result.ExternalPostId,
                Title = distribution.ContentItem.Title,
                PublishedAt = DateTime.UtcNow
            });
            
            await _auditService.LogAsync(
                "system",
                "content_published",
                nameof(ContentDistribution),
                distributionId.ToString(),
                successMetadata,
                cancellationToken);
        }
        else
        {
            distribution.MarkFailed(result.ErrorMessage ?? "Unknown error");
            _logger.LogWarning("Failed to publish distribution {DistributionId} to {Platform}: {Error}", 
                distributionId, distribution.Platform, result.ErrorMessage);
                
            // Audit failed publish
            var failureMetadata = JsonSerializer.Serialize(new
            {
                DistributionId = distributionId,
                Platform = distribution.Platform.ToString(),
                Error = result.ErrorMessage,
                IsRetryable = result.IsRetryable,
                Title = distribution.ContentItem.Title,
                FailedAt = DateTime.UtcNow
            });
            
            await _auditService.LogAsync(
                "system",
                "content_publish_failed",
                nameof(ContentDistribution),
                distributionId.ToString(),
                failureMetadata,
                cancellationToken);
                
            // Don't throw if not retryable
            if (result.IsRetryable)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }



    private async Task HandleRetryAsync(Guid distributionId, string errorMessage, IJobExecutionContext context)
    {
        try
        {
            var distribution = await _dbContext.ContentDistributions
                .FirstOrDefaultAsync(d => d.Id == distributionId, context.CancellationToken);

            if (distribution is null)
                return;

            var currentAttempt = context.RefireCount + 1;
            var maxRetries = distribution.Window.MaxRetryCount;

            if (currentAttempt >= maxRetries)
            {
                _logger.LogError("Max retry attempts ({MaxRetries}) reached for distribution {DistributionId}", maxRetries, distributionId);
                distribution.MarkFailed($"Max retries exceeded. Last error: {errorMessage}");
                await _dbContext.SaveChangesAsync(context.CancellationToken);
                return;
            }

            // Calculate exponential backoff delay
            var baseDelaySeconds = (int)(distribution.Window.RetryInterval?.TotalSeconds ?? 300); // Default 5 minutes
            var exponentialDelay = TimeSpan.FromSeconds(baseDelaySeconds * Math.Pow(2, currentAttempt - 1));
            var maxDelay = TimeSpan.FromHours(4); // Cap at 4 hours
            var actualDelay = exponentialDelay > maxDelay ? maxDelay : exponentialDelay;

            _logger.LogInformation("Scheduling retry {Attempt}/{MaxRetries} for distribution {DistributionId} in {Delay}", 
                currentAttempt, maxRetries, distributionId, actualDelay);

            // Schedule retry job
            var retryJobDetail = context.JobDetail.Clone();
            var retryTrigger = TriggerBuilder.Create()
                .WithIdentity($"retry-{distributionId}-{currentAttempt}")
                .StartAt(DateTimeOffset.UtcNow.Add(actualDelay))
                .Build();

            await context.Scheduler.ScheduleJob(retryJobDetail, retryTrigger, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule retry for distribution {DistributionId}", distributionId);
        }
    }
}
