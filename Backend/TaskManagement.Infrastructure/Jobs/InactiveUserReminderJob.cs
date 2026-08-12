using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.Infrastructure.Jobs;
public class InactiveUserReminderJob : IInactiveUserReminderJob
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<InactiveUserReminderJob> _logger;
    public InactiveUserReminderJob(
        ApplicationDbContext db, 
        IEmailService emailService, 
        ILogger<InactiveUserReminderJob> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Hangfire Job Başladı: InactiveUserReminderJob");
        // Son 7 gündür sisteme girmemiş aktif kullanıcıları bul
        var thresholdDate = DateTime.UtcNow.AddDays(-7);
        var inactiveUsers = await _db.Users
            .Where(u => u.IsActive && 
                        (u.LastLoginAt == null || u.LastLoginAt < thresholdDate))
            .ToListAsync(ct);
        if (!inactiveUsers.Any())
        {
            _logger.LogInformation("Hatırlatma gönderilecek aktif olmayan kullanıcı bulunamadı.");
            return;
        }
        foreach (var user in inactiveUsers)
        {
            var subject = "Task Management - Sizi Özledik!";
            var body = $"Merhaba {user.FirstName}, sistemde bekleyen görevleriniz olabilir. Kontrol etmek için lütfen giriş yapın.";
            
            await _emailService.SendReminderAsync(user.Email, subject, body, ct);
        }
        _logger.LogInformation($"Hangfire Job Bitti: Toplam {inactiveUsers.Count} kullanıcıya e-posta (mock) gönderildi.");
    }
}
