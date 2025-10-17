using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Application.Features.Content.Common;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Content.Queries;

public sealed record ContentPipelineOwnerDto(Guid? Id, string? Name);

public sealed record ContentPipelineChannelDto(string Id, string Name);

public sealed record ContentPipelineItemDto(
    Guid Id,
    Guid TeamId,
    string Title,
    string Status,
    ContentPipelineChannelDto Channel,
    ContentPipelineOwnerDto Owner,
    DateTime UpdatedOnUtc,
    DateTime? DueOnUtc,
    DateTime? ScheduledOnUtc);

public sealed record GetContentPipelineItemsQuery(
    Guid? TeamId,
    Guid? OwnerId,
    ContentStatus? Status,
    int PageNumber,
    int PageSize,
    string? Search) : IRequest<Result<PagedResult<ContentPipelineItemDto>>>;

public sealed class GetContentPipelineItemsQueryHandler : IRequestHandler<GetContentPipelineItemsQuery, Result<PagedResult<ContentPipelineItemDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContentPipelineItemsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<ContentPipelineItemDto>>> Handle(GetContentPipelineItemsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 25 : request.PageSize;

        var query = _dbContext.ContentItems
            .AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.Distributions)
            .Where(item => !item.IsDeleted);

        if (request.TeamId.HasValue && request.TeamId.Value != Guid.Empty)
        {
            query = query.Where(item => item.TeamId == request.TeamId.Value);
        }

        if (request.OwnerId.HasValue && request.OwnerId.Value != Guid.Empty)
        {
            query = query.Where(item => item.AuthorId == request.OwnerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(item => item.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(item => EF.Functions.ILike(item.Title, $"%{term}%") || EF.Functions.ILike(item.Body, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(item => item.ModifiedOnUtc ?? item.CreatedOnUtc)
            .ThenByDescending(item => item.CreatedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    var mapped = items.Select(ContentPipelineMapper.MapToDto).ToArray();

        var paged = new PagedResult<ContentPipelineItemDto>
        {
            Items = mapped,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return Result<PagedResult<ContentPipelineItemDto>>.Success(paged);
    }

}
