using System;
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
}
