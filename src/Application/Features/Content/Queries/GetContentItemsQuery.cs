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

public sealed record ContentItemDto(Guid Id, string Title, string Body, string? Caption, ContentStatus Status, IEnumerable<string> Hashtags, DateTime CreatedOnUtc);

public sealed record GetContentItemsQuery(Guid TeamId, int PageNumber = 1, int PageSize = 20) : IRequest<Result<PagedResult<ContentItemDto>>>;

public sealed class GetContentItemsQueryHandler : IRequestHandler<GetContentItemsQuery, Result<PagedResult<ContentItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContentItemsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<ContentItemDto>>> Handle(GetContentItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ContentItems
            .Where(c => c.TeamId == request.TeamId)
            .OrderByDescending(c => c.CreatedOnUtc)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ContentItemDto(
                c.Id,
                c.Title,
                c.Body,
                c.Caption,
                c.Status,
                c.Hashtags,
                c.CreatedOnUtc))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ContentItemDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ContentItemDto>>.Success(result);
    }
}
