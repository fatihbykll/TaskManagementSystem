using Microsoft.AspNetCore.SignalR;
using TaskManagement.Application.Interfaces;
using TaskManagement.API.Hubs;
namespace TaskManagement.API.Services;
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    public SignalRNotificationService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }
    public async Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct = default)
    {
        await _hub.Clients.Group($"user-{userId}").SendAsync(eventName, payload, ct);
    }
    public async Task SendToAllAsync(string eventName, object payload, CancellationToken ct = default)
    {
        await _hub.Clients.All.SendAsync(eventName, payload, ct);
    }
}
