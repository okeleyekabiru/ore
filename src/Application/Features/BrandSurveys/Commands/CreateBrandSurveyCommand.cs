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
using Ore.Domain.Enums;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed record SurveyQuestionDto(string Prompt, SurveyQuestionType Type, int Order, IEnumerable<string> Options);

public sealed record CreateBrandSurveyCommand(Guid TeamId, string Title, string Description, IEnumerable<SurveyQuestionDto> Questions)
    : IRequest<Result<Guid>>;

public sealed class CreateBrandSurveyCommandHandler : IRequestHandler<CreateBrandSurveyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateBrandSurveyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateBrandSurveyCommand request, CancellationToken cancellationToken)
    {
        var teamExists = await _dbContext.Teams.AnyAsync(t => t.Id == request.TeamId, cancellationToken);

        if (!teamExists)
        {
            return Result<Guid>.Failure("Team not found");
        }

        var survey = new BrandSurvey(request.TeamId, request.Title, request.Description);

        foreach (var question in request.Questions.OrderBy(q => q.Order))
        {
            survey.AddQuestion(question.Prompt, question.Type, question.Order, question.Options);
        }

        _dbContext.BrandSurveys.Add(survey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(survey.Id);
    }
}
