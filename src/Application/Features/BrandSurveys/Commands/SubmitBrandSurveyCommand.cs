using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed record SurveyAnswerInput(Guid QuestionId, string Value, string? Metadata);

public sealed record BrandVoiceProfileInput(string Voice, string Tone, string Audience, string Goals, string Competitors, IEnumerable<string> Keywords);

public sealed record SubmitBrandSurveyCommand(Guid SurveyId, Guid UserId, IEnumerable<SurveyAnswerInput> Answers, BrandVoiceProfileInput? VoiceProfile)
    : IRequest<Result<Guid>>;

public sealed class SubmitBrandSurveyCommandHandler : IRequestHandler<SubmitBrandSurveyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public SubmitBrandSurveyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(SubmitBrandSurveyCommand request, CancellationToken cancellationToken)
    {
        var survey = await _dbContext.BrandSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == request.SurveyId, cancellationToken);

        if (survey is null)
        {
            return Result<Guid>.Failure("Survey not found");
        }

        if (!survey.IsActive)
        {
            return Result<Guid>.Failure("Survey is not currently active.");
        }

        var answers = request.Answers.ToList();
        var surveyQuestionIds = survey.Questions.Select(q => q.Id).ToHashSet();

        if (answers.Any(answer => !surveyQuestionIds.Contains(answer.QuestionId)))
        {
            return Result<Guid>.Failure("One or more answers reference questions that do not belong to this survey.");
        }

        var duplicateQuestionAnswers = answers
            .GroupBy(answer => answer.QuestionId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateQuestionAnswers.Length > 0)
        {
            return Result<Guid>.Failure("Each survey question can only be answered once.");
        }

        var submission = new BrandSurveySubmission(request.SurveyId, request.UserId);

        foreach (var answer in answers)
        {
            submission.AddAnswer(answer.QuestionId, answer.Value, answer.Metadata);
        }

        _dbContext.BrandSurveySubmissions.Add(submission);

        if (request.VoiceProfile is not null)
        {
            var voice = BrandVoiceProfile.Create(
                request.VoiceProfile.Voice,
                request.VoiceProfile.Tone,
                request.VoiceProfile.Audience,
                request.VoiceProfile.Goals,
                request.VoiceProfile.Competitors,
                request.VoiceProfile.Keywords);

            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == survey.TeamId, cancellationToken);
            team?.SetBrandVoice(voice);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(submission.Id);
    }
}
