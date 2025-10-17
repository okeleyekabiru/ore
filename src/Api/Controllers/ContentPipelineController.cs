using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Api.Contracts.ContentPipeline;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Common.Models;
using Ore.Application.Features.Content.Commands;
using Ore.Application.Features.Content.Queries;
using Ore.Domain.Enums;

namespace Ore.Api.Controllers;

[Authorize]
[Route("api/content-pipeline")]
public sealed class ContentPipelineController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public ContentPipelineController(IMediator mediator, ICurrentUserService currentUserService)
        : base(mediator)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] Guid? teamId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetContentPipelineSummaryQuery(teamId), cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<IReadOnlyCollection<ContentPipelineSummaryResponse>>.Failure([.. result.Errors]));
        }

        var response = result.Value
            .Select(entry => new ContentPipelineSummaryResponse(entry.Status, entry.Count))
            .ToArray();

        return FromResult(Result<IReadOnlyCollection<ContentPipelineSummaryResponse>>.Success(response));
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] ContentPipelineItemsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var status = TryParseStatus(parameters.Status);

        var query = new GetContentPipelineItemsQuery(
            parameters.TeamId,
            parameters.OwnerId,
            status,
            parameters.Page,
            parameters.PageSize,
            parameters.Search);

        var result = await Mediator.Send(query, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<ContentPipelineItemsResponse>.Failure([.. result.Errors]));
        }

        var response = new ContentPipelineItemsResponse(
            result.Value.Items.Select(item => new ContentPipelineItemResponse(
                item.Id,
                item.TeamId,
                item.Title,
                item.Status,
                new ContentPipelineChannelResponse(item.Channel.Id, item.Channel.Name),
                new ContentPipelineOwnerResponse(item.Owner.Id, item.Owner.Name),
                item.UpdatedOnUtc,
                item.DueOnUtc,
                item.ScheduledOnUtc)).ToArray(),
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalCount,
            (int)Math.Ceiling(result.Value.TotalCount / (double)result.Value.PageSize));

        return FromResult(Result<ContentPipelineItemsResponse>.Success(response));
    }

    [HttpPatch("items/{contentId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid contentId, [FromBody] UpdateContentPipelineStatusRequest request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.GetUserId();
        var status = TryParseStatus(request.Status);

        if (status is null)
        {
            return FromResult(Result<Guid>.Failure("Unsupported status value provided."));
        }

        var command = new UpdateContentPipelineStatusCommand(
            contentId,
            status.Value,
            actorId,
            request.ScheduledOnUtc,
            request.Reason,
            TryParsePlatform(request.Platform));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "Content status updated.");
    }

    [HttpPatch("items/status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateContentPipelineStatusRequest request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.GetUserId();

        if (request.ItemIds.Count == 0)
        {
            return FromResult(Result<int>.Failure("No content item ids were provided."));
        }

        var status = TryParseStatus(request.Status);
        if (status is null)
        {
            return FromResult(Result<int>.Failure("Unsupported status value provided."));
        }

        var failures = new List<string>();

        foreach (var itemId in request.ItemIds)
        {
            var command = new UpdateContentPipelineStatusCommand(
                itemId,
                status.Value,
                actorId,
                request.ScheduledOnUtc,
                request.Reason,
                TryParsePlatform(request.Platform));

            var result = await Mediator.Send(command, cancellationToken);
            if (!result.Succeeded)
            {
                failures.AddRange(result.Errors);
            }
        }

        return failures.Count == 0
            ? FromResult(Result<string>.Success("Content status updated."))
            : FromResult(Result<string>.Failure([.. failures]));
    }

    private static ContentStatus? TryParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ContentStatus>(value, ignoreCase: true, out var status) ? status : null;
    }

    private static PlatformType? TryParsePlatform(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<PlatformType>(value, ignoreCase: true, out var platform) ? platform : null;
    }
}
