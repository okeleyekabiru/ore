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
[Route("api/brand-surveys")]
public sealed class BrandSurveysController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public BrandSurveysController(IMediator mediator, ICurrentUserService currentUserService)
        : base(mediator)
    {
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSurvey(
        [FromBody] CreateBrandSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBrandSurveyCommand(
            request.TeamId,
            request.Title,
            request.Description,
            request.Questions.Select(q => new SurveyQuestionDto(q.Prompt, q.Type, q.Order, q.Options ?? [])));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "Brand survey created.");
    }

    [HttpPut("{surveyId:guid}")]
    public async Task<IActionResult> UpdateSurvey(
        Guid surveyId,
        [FromBody] UpdateBrandSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBrandSurveyCommand(
            surveyId,
            request.Title,
            request.Description,
            request.Questions.Select(q => new SurveyQuestionDto(q.Prompt, q.Type, q.Order, q.Options ?? [])));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "Brand survey updated.");
    }

    [HttpGet]
    public async Task<IActionResult> ListSurveys(
        [FromQuery] Guid? teamId,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ListBrandSurveysQuery(teamId, includeInactive), cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<IReadOnlyCollection<BrandSurveySummaryResponse>>.Failure([.. result.Errors]));
        }

        var response = result.Value
            .Select(s => new BrandSurveySummaryResponse(
                s.Id,
                s.TeamId,
                s.Title,
                s.Description,
                s.IsActive,
                s.QuestionCount,
                s.CreatedOnUtc,
                s.ModifiedOnUtc))
            .ToArray();

        return FromResult(Result<IReadOnlyCollection<BrandSurveySummaryResponse>>.Success(response));
    }

    [HttpGet("{surveyId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSurvey(Guid surveyId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetBrandSurveyQuery(surveyId), cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return FromResult(Result<BrandSurveyDetailsResponse>.Failure([.. result.Errors]));
        }

        var response = new BrandSurveyDetailsResponse(
            result.Value.Id,
            result.Value.TeamId,
            result.Value.Title,
            result.Value.Description,
            result.Value.IsActive,
            [.. result.Value.Questions.Select(q => new BrandSurveyQuestionDetails(q.Id, q.Prompt, q.Type, q.Order, q.Options))]);

        return FromResult(Result<BrandSurveyDetailsResponse>.Success(response));
    }

    [HttpPost("{surveyId:guid}/submissions")]
    public async Task<IActionResult> Submit(
        Guid surveyId,
        [FromBody] SubmitBrandSurveyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? _currentUserService.GetUserId();

        var command = new SubmitBrandSurveyCommand(
            surveyId,
            userId,
            request.Answers.Select(a => new SurveyAnswerInput(a.QuestionId, a.Value, a.Metadata)),
            request.VoiceProfile is null
                ? null
                : new BrandVoiceProfileInput(
                    request.VoiceProfile.Voice,
                    request.VoiceProfile.Tone,
                    request.VoiceProfile.Audience,
                    request.VoiceProfile.Keywords ?? []));

        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result, "Survey submitted.");
    }

    [HttpPost("{surveyId:guid}/activate")]
    public async Task<IActionResult> ActivateSurvey(Guid surveyId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateBrandSurveyStatusCommand(surveyId, true), cancellationToken);
        return FromResult(result, "Brand survey activated.");
    }

    [HttpPost("{surveyId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateSurvey(Guid surveyId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateBrandSurveyStatusCommand(surveyId, false), cancellationToken);
        return FromResult(result, "Brand survey deactivated.");
    }
}
