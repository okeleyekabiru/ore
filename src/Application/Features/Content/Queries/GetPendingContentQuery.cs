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

public sealed record GetPendingContentQuery(Guid TeamId) : IRequest<Result<IReadOnlyCollection<PendingContentDto>>>;

public sealed record PendingContentDto(
    Guid Id,
    string Title,
    string Body,
    string? Caption,
    IReadOnlyCollection<string> Hashtags,
    DateTime SubmittedOnUtc,
    PendingContentAuthorDto Author
);

public sealed record PendingContentAuthorDto(
    Guid Id,
    string Name,
    string Email
);

public sealed class GetPendingContentQueryHandler : IRequestHandler<GetPendingContentQuery, Result<IReadOnlyCollection<PendingContentDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPendingContentQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<PendingContentDto>>> Handle(GetPendingContentQuery request, CancellationToken cancellationToken)
    {
        var pendingContent = await _dbContext.ContentItems
            .Where(c => c.TeamId == request.TeamId && c.Status == ContentStatus.PendingApproval)
            .Include(c => c.Author)
            .OrderByDescending(c => c.ModifiedOnUtc ?? c.CreatedOnUtc)
            .Select(c => new PendingContentDto(
                c.Id,
                c.Title,
                c.Body,
                c.Caption,
                c.Hashtags,
                c.ModifiedOnUtc ?? c.CreatedOnUtc,
                new PendingContentAuthorDto(
                    c.Author!.Id,
                    c.Author.FullName,
                    c.Author.Email
                )
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<PendingContentDto>>.Success(pendingContent);
    }
}