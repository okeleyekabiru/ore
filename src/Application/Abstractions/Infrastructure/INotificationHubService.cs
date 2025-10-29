namespace Ore.Application.Abstractions.Infrastructure;

public interface INotificationHubService
{
    Task SendToUserAsync(string userId, string method, object data, CancellationToken cancellationToken = default);
    Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default);
    Task SendToAllAsync(string method, object data, CancellationToken cancellationToken = default);
}