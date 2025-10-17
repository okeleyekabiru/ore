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

namespace Ore.Application.Features.Content.Queries;

public sealed record ContentPipelineSummaryDto(string Status, int Count);

public sealed record GetContentPipelineSummaryQuery(Guid? TeamId) : IRequest<Result<IReadOnlyCollection<ContentPipelineSummaryDto>>>;

public sealed class GetContentPipelineSummaryQueryHandler : IRequestHandler<GetContentPipelineSummaryQuery, Result<IReadOnlyCollection<ContentPipelineSummaryDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContentPipelineSummaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<ContentPipelineSummaryDto>>> Handle(GetContentPipelineSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ContentItems.AsNoTracking();

        if (request.TeamId.HasValue && request.TeamId.Value != Guid.Empty)
        {
            query = query.Where(item => item.TeamId == request.TeamId.Value);
        }

        var snapshot = await query
            .GroupBy(item => item.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var result = snapshot
            .Select(entry => new ContentPipelineSummaryDto(entry.Status.ToString(), entry.Count))
            .ToArray();

        return Result<IReadOnlyCollection<ContentPipelineSummaryDto>>.Success(result);
    }
}
