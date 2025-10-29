using Microsoft.AspNetCore.SignalR;
using Ore.Api.Hubs;
using Ore.Application.Abstractions.Infrastructure;

namespace Ore.Api.Services;

public sealed class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string method, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"User_{userId}").SendAsync(method, data, cancellationToken);
    }

    public async Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(method, data, cancellationToken);
    }

    public async Task SendToAllAsync(string method, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync(method, data, cancellationToken);
    }
}