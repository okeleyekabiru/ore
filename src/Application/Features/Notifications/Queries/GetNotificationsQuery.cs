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

namespace Ore.Application.Features.Notifications.Queries;

public sealed record NotificationDto(Guid Id, NotificationType Type, string Message, bool IsRead, DateTime CreatedOnUtc);

public sealed record GetNotificationsQuery(Guid RecipientId, int PageNumber = 1, int PageSize = 20) : IRequest<Result<PagedResult<NotificationDto>>>;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetNotificationsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Where(n => n.RecipientId == request.RecipientId)
            .OrderByDescending(n => n.CreatedOnUtc)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Message, n.IsRead, n.CreatedOnUtc))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<NotificationDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<NotificationDto>>.Success(result);
    }
}
