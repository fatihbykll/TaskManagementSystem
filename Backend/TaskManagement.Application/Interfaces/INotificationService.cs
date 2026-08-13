namespace TaskManagement.Application.Interfaces;
public interface INotificationService
{
    /// <summary>Belirli bir kullanıcıya özel bildirim gönderir.</summary>
    Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct = default);
    /// <summary>Bağlı tüm istemcilere bildirim gönderir.</summary>
    Task SendToAllAsync(string eventName, object payload, CancellationToken ct = default);
}
