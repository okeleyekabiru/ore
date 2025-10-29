using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Api.Contracts.Content;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Common.Models;
using Ore.Application.Features.Content.Commands;
using Ore.Application.Features.Content.Queries;
using Ore.Domain.Enums;

namespace Ore.Api.Controllers;

[Authorize]
[Route("api/content")]
public sealed class ContentController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public ContentController(IMediator mediator, ICurrentUserService currentUserService)
        : base(mediator)
    {
        _currentUserService = currentUserService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateContentRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PlatformType>(request.Platform, ignoreCase: true, out var platform))
        {
            return FromResult(Result<GenerateContentResponse>.Failure("Unsupported platform value provided."));
        }

        var actorId = _currentUserService.GetUserId();

        var command = new GenerateContentCommand(
            request.BrandId,
            actorId,
            request.Topic,
            request.Tone,
            platform);

        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<GenerateContentResponse>.Failure([.. result.Errors]));
        }

        var response = new GenerateContentResponse(
            result.Value.Caption,
            result.Value.Hashtags,
            result.Value.ImageIdea);

        return FromResult(Result<GenerateContentResponse>.Success(response), "Content suggestion generated.");
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitContentRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.GetUserId();

        var command = new SubmitContentForApprovalCommand(request.ContentId, actorId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return FromResult(Result<Guid>.Failure([.. result.Errors]));
        }

        return FromResult(Result<Guid>.Success(result.Value), "Content submitted for approval.");
    }

    [HttpPost("approve")]
    public async Task<IActionResult> Approve(
        [FromBody] ApproveContentRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.GetUserId();

        var command = new ApproveContentCommand(request.ContentId, actorId, request.Comments);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return FromResult(Result<Guid>.Failure([.. result.Errors]));
        }

        return FromResult(Result<Guid>.Success(result.Value), "Content approved successfully.");
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] Guid teamId,
        CancellationToken cancellationToken)
    {
        if (teamId == Guid.Empty)
        {
            return FromResult(Result<PendingContentResponse[]>.Failure("Team ID is required."));
        }

        var query = new GetPendingContentQuery(teamId);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<PendingContentResponse[]>.Failure([.. result.Errors]));
        }

        var response = result.Value.Select(dto => new PendingContentResponse(
            dto.Id,
            dto.Title,
            dto.Body,
            dto.Caption,
            dto.Hashtags,
            dto.SubmittedOnUtc,
            new PendingContentAuthorResponse(
                dto.Author.Id,
                dto.Author.Name,
                dto.Author.Email
            )
        )).ToArray();

        return FromResult(Result<PendingContentResponse[]>.Success(response), "Pending content retrieved.");
    }
}
