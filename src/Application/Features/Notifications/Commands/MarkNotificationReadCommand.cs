using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;

namespace Ore.Application.Features.Notifications.Commands;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Result<Guid>>;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public MarkNotificationReadCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification is null)
        {
            return Result<Guid>.Failure("Notification not found");
        }

        notification.MarkRead();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(notification.Id);
    }
}
