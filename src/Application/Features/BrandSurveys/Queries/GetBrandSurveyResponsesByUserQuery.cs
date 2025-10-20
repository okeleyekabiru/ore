using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Enums;

namespace Ore.Application.Features.BrandSurveys.Queries;

public sealed record SurveyResponseAnswer(Guid QuestionId, string Prompt, SurveyQuestionType Type, string Value, string? Metadata);

public sealed record BrandSurveySubmissionResponse(
    Guid SubmissionId,
    Guid SurveyId,
    Guid UserId,
    string SurveyTitle,
    string SurveyCategory,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc,
    IReadOnlyCollection<SurveyResponseAnswer> Answers);

public sealed record GetBrandSurveyResponsesByUserQuery(Guid UserId)
    : IRequest<Result<IReadOnlyCollection<BrandSurveySubmissionResponse>>>;

public sealed class GetBrandSurveyResponsesByUserQueryHandler : IRequestHandler<GetBrandSurveyResponsesByUserQuery, Result<IReadOnlyCollection<BrandSurveySubmissionResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBrandSurveyResponsesByUserQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<BrandSurveySubmissionResponse>>> Handle(GetBrandSurveyResponsesByUserQuery request, CancellationToken cancellationToken)
    {
        var submissionEntities = await _dbContext.BrandSurveySubmissions
            .Where(submission => submission.UserId == request.UserId)
            .Include(submission => submission.Answers)
            .AsNoTracking()
            .OrderByDescending(submission => submission.ModifiedOnUtc ?? submission.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        if (submissionEntities.Count == 0)
        {
            return Result<IReadOnlyCollection<BrandSurveySubmissionResponse>>.Success(Array.Empty<BrandSurveySubmissionResponse>());
        }

        var surveyIds = submissionEntities
            .Select(submission => submission.SurveyId)
            .Distinct()
            .ToArray();

        var surveyLookup = await _dbContext.BrandSurveys
            .Where(survey => surveyIds.Contains(survey.Id))
            .Include(survey => survey.Questions)
            .AsNoTracking()
            .ToDictionaryAsync(survey => survey.Id, cancellationToken);

        var responses = submissionEntities
            .Select(submission =>
            {
                surveyLookup.TryGetValue(submission.SurveyId, out var survey);

                var answers = submission.Answers
                    .Select(answer =>
                    {
                        var question = survey?.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                        var prompt = question?.Prompt ?? string.Empty;
                        var type = question?.Type ?? SurveyQuestionType.Text;

                        return new SurveyResponseAnswer(
                            answer.QuestionId,
                            prompt,
                            type,
                            answer.Value,
                            answer.Metadata);
                    })
                    .ToArray();

                var title = survey?.Title ?? string.Empty;
                var category = survey?.Category ?? string.Empty;

                return new BrandSurveySubmissionResponse(
                    submission.Id,
                    submission.SurveyId,
                    submission.UserId,
                    title,
                    category,
                    submission.CreatedOnUtc,
                    submission.ModifiedOnUtc,
                    answers);
            })
            .ToArray();

        return Result<IReadOnlyCollection<BrandSurveySubmissionResponse>>.Success(responses);
    }
}
