using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Api.Contracts.BrandSurveys;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Common.Models;
using Ore.Application.Features.BrandSurveys.Commands;
using Ore.Application.Features.BrandSurveys.Queries;

namespace Ore.Api.Controllers;

[Authorize]
[Route("api/survey-response")]
public sealed class SurveyResponsesController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public SurveyResponsesController(IMediator mediator, ICurrentUserService currentUserService)
        : base(mediator)
    {
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSurveyResponse([FromBody] CreateSurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _currentUserService.GetUserId();

        var command = new SubmitBrandSurveyCommand(
            request.SurveyId,
            userId,
            request.Answers.Select(answer => new SurveyAnswerInput(answer.QuestionId, answer.Value, answer.Metadata)),
            request.VoiceProfile is null
                ? null
                : new BrandVoiceProfileInput(
                    request.VoiceProfile.Voice,
                    request.VoiceProfile.Tone,
                    request.VoiceProfile.Audience,
                    request.VoiceProfile.Goals,
                    request.VoiceProfile.Competitors,
                    request.VoiceProfile.Keywords ?? Array.Empty<string>()));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "Survey response submitted.");
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetResponsesByUser(Guid userId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetBrandSurveyResponsesByUserQuery(userId), cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<IReadOnlyCollection<SurveyResponseDetailsResponse>>.Failure([.. result.Errors]));
        }

        var responses = result.Value
            .Select(entry => new SurveyResponseDetailsResponse(
                entry.SubmissionId,
                entry.SurveyId,
                entry.UserId,
                entry.SurveyTitle,
                entry.SurveyCategory,
                entry.CreatedOnUtc,
                entry.ModifiedOnUtc,
                entry.Answers.Select(answer => new SurveyResponseAnswerDetails(
                    answer.QuestionId,
                    answer.Prompt,
                    answer.Type,
                    answer.Value,
                    answer.Metadata))))
            .ToArray();

        return FromResult(Result<IReadOnlyCollection<SurveyResponseDetailsResponse>>.Success(responses));
    }
}
