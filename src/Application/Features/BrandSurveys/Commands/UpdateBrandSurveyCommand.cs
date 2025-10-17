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

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed record UpdateBrandSurveyCommand(
    Guid SurveyId,
    string Title,
    string Description,
    IEnumerable<SurveyQuestionDto> Questions) : IRequest<Result<Guid>>;

public sealed class UpdateBrandSurveyCommandHandler : IRequestHandler<UpdateBrandSurveyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBrandSurveyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(UpdateBrandSurveyCommand request, CancellationToken cancellationToken)
    {
        var survey = await _dbContext.BrandSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == request.SurveyId, cancellationToken);

        if (survey is null)
        {
            return Result<Guid>.Failure("Survey not found");
        }

        survey.UpdateDetails(request.Title, request.Description);

        var sanitizedQuestions = request.Questions
            .Select(q => (
                Prompt: q.Prompt,
                q.Type,
                q.Order,
                Options: q.Options?.Where(option => !string.IsNullOrWhiteSpace(option))
                    .Select(option => option.Trim())
                    .ToArray() ?? Array.Empty<string>()))
            .OrderBy(q => q.Order)
            .ToList();

        var trackedQuestions = survey.Questions
            .OrderBy(q => q.Order)
            .ToList();

        var pairedCount = Math.Min(trackedQuestions.Count, sanitizedQuestions.Count);

        for (var i = 0; i < pairedCount; i++)
        {
            var existingQuestion = trackedQuestions[i];
            var incoming = sanitizedQuestions[i];

            if (existingQuestion.Options.Any())
            {
                _dbContext.SurveyOptions.RemoveRange(existingQuestion.Options);
            }

            existingQuestion.Update(incoming.Prompt, incoming.Type, incoming.Order, incoming.Options);
        }

        if (trackedQuestions.Count > sanitizedQuestions.Count)
        {
            var toRemove = trackedQuestions.Skip(sanitizedQuestions.Count).ToArray();

            var orphanOptions = toRemove.SelectMany(q => q.Options).ToArray();
            if (orphanOptions.Length > 0)
            {
                _dbContext.SurveyOptions.RemoveRange(orphanOptions);
            }

            survey.RemoveQuestions(toRemove.Select(q => q.Id));
            _dbContext.SurveyQuestions.RemoveRange(toRemove);
        }

        if (sanitizedQuestions.Count > trackedQuestions.Count)
        {
            foreach (var question in sanitizedQuestions.Skip(trackedQuestions.Count))
            {
                survey.AddQuestion(question.Prompt, question.Type, question.Order, question.Options);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(survey.Id);
    }
}
