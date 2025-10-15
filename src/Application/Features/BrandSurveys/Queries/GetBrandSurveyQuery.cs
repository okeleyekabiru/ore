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

public sealed record SurveyQuestionResponse(Guid Id, string Prompt, SurveyQuestionType Type, int Order, IEnumerable<string> Options);

public sealed record BrandSurveyResponse(Guid Id, string Title, string Description, IEnumerable<SurveyQuestionResponse> Questions);

public sealed record GetBrandSurveyQuery(Guid SurveyId) : IRequest<Result<BrandSurveyResponse>>;

public sealed class GetBrandSurveyQueryHandler : IRequestHandler<GetBrandSurveyQuery, Result<BrandSurveyResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBrandSurveyQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BrandSurveyResponse>> Handle(GetBrandSurveyQuery request, CancellationToken cancellationToken)
    {
        var survey = await _dbContext.BrandSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SurveyId, cancellationToken);

        if (survey is null)
        {
            return Result<BrandSurveyResponse>.Failure("Survey not found");
        }

        var response = new BrandSurveyResponse(
            survey.Id,
            survey.Title,
            survey.Description,
            survey.Questions
                .OrderBy(q => q.Order)
                .Select(q => new SurveyQuestionResponse(
                    q.Id,
                    q.Prompt,
                    q.Type,
                    q.Order,
                    q.Options.Select(o => o.Value))));

        return Result<BrandSurveyResponse>.Success(response);
    }
}
