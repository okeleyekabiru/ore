using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;

namespace Ore.Application.Features.BrandSurveys.Queries;

public sealed record BrandSurveySummary(
    Guid Id,
    Guid TeamId,
    string Title,
    string Description,
    string Category,
    bool IsActive,
    int QuestionCount,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);

public sealed record ListBrandSurveysQuery(Guid? TeamId, bool IncludeInactive)
    : IRequest<Result<IReadOnlyCollection<BrandSurveySummary>>>;

public sealed class ListBrandSurveysQueryHandler : IRequestHandler<ListBrandSurveysQuery, Result<IReadOnlyCollection<BrandSurveySummary>>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListBrandSurveysQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<BrandSurveySummary>>> Handle(ListBrandSurveysQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.BrandSurveys
            .Include(s => s.Questions)
            .AsNoTracking()
            .AsQueryable();

        if (request.TeamId.HasValue)
        {
            query = query.Where(s => s.TeamId == request.TeamId.Value);
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var surveys = await query
            .OrderByDescending(s => s.ModifiedOnUtc ?? s.CreatedOnUtc)
            .Select(s => new BrandSurveySummary(
                s.Id,
                s.TeamId,
                s.Title,
                s.Description,
                s.Category,
                s.IsActive,
                s.Questions.Count,
                s.CreatedOnUtc,
                s.ModifiedOnUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<BrandSurveySummary>>.Success(surveys);
    }
}
