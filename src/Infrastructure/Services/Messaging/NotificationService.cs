using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Infrastructure.Services.Messaging;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IApplicationDbContext dbContext, ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task DispatchAsync(Guid recipientId, NotificationType type, string message, CancellationToken cancellationToken = default)
    {
        var notification = new Notification(recipientId, type, message);
        await _dbContext.Notifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Notification dispatched to {Recipient} with type {Type}.", recipientId, type);
    }
}
