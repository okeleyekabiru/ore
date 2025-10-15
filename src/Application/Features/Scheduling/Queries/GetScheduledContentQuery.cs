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

namespace Ore.Application.Features.Scheduling.Queries;

public sealed record ScheduledContentDto(Guid DistributionId, Guid ContentId, PlatformType Platform, DateTime PublishOnUtc, DateTime? PublishedOnUtc, string? FailureReason);

public sealed record GetScheduledContentQuery(Guid TeamId, int PageNumber = 1, int PageSize = 20) : IRequest<Result<PagedResult<ScheduledContentDto>>>;

public sealed class GetScheduledContentQueryHandler : IRequestHandler<GetScheduledContentQuery, Result<PagedResult<ScheduledContentDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetScheduledContentQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<ScheduledContentDto>>> Handle(GetScheduledContentQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ContentDistributions
            .Where(d => d.ContentItem != null && d.ContentItem.TeamId == request.TeamId)
            .OrderByDescending(d => d.Window.PublishOnUtc)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new ScheduledContentDto(
                d.Id,
                d.ContentItemId,
                d.Platform,
                d.Window.PublishOnUtc,
                d.PublishedOnUtc,
                d.FailureReason))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ScheduledContentDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ScheduledContentDto>>.Success(result);
    }
}
