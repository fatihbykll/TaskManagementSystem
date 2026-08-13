using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace TaskManagement.API.Hubs;
/// <summary>
/// Kimlik doğrulaması zorunlu; yalnızca JWT token'ı geçerli istemciler bağlanabilir.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Bağlantı kurulduğunda kullanıcıyı kendi ID'siyle eşleştirilen gruba ekler.
    /// Böylece sunucudan sadece o kullanıcıya özel mesaj gönderilebilir.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnDisconnectedAsync(exception);
    }
}
