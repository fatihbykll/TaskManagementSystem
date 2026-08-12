using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
namespace TaskManagement.Infrastructure.Services;
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;
    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }
    public Task SendReminderAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // Gerçekte SMTP kullanılmayacağı için loglama yapıyoruz
        _logger.LogInformation("📧 [MOCK EMAIL] Alıcı: {To} | Konu: {Subject} | Mesaj: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
