using System;
using System.Threading;
using System.Threading.Tasks;
using Ore.Domain.Enums;

namespace Ore.Application.Abstractions.Messaging;

public interface INotificationService
{
    Task DispatchAsync(Guid recipientId, NotificationType type, string message, CancellationToken cancellationToken = default);
}
