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

        var existingQuestions = survey.Questions.ToList();
        if (existingQuestions.Count > 0)
        {
            var optionEntries = existingQuestions.SelectMany(q => q.Options).ToList();
            if (optionEntries.Count > 0)
            {
                _dbContext.SurveyOptions.RemoveRange(optionEntries);
            }

            survey.RemoveQuestions(existingQuestions.Select(q => q.Id));
            _dbContext.SurveyQuestions.RemoveRange(existingQuestions);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

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

        foreach (var question in sanitizedQuestions)
        {
            survey.AddQuestion(question.Prompt, question.Type, question.Order, question.Options);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(survey.Id);
    }
}
