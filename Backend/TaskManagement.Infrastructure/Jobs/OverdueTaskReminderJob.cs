using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.Infrastructure.Jobs;
public class OverdueTaskReminderJob
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    public OverdueTaskReminderJob(ApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }
    public async Task CheckAndSendRemindersAsync()
    {
        var thresholdDate = DateTime.UtcNow.AddDays(1);
        var overdueTasks = await _db.Tasks
            .Where(t => t.Status != TaskItemStatus.Completed && t.Status != TaskItemStatus.Cancelled)
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= thresholdDate.Date)
            .ToListAsync();
        foreach (var task in overdueTasks)
        {
            var user = await _db.Users.FindAsync(task.UserId);
            if (user != null)
            {
                await _emailService.SendReminderAsync(
                    user.Email,
                    $"Hatırlatma: '{task.Title}' görevinin süresi dolmak üzere!",
                    $"Merhaba {user.FirstName}, '{task.Title}' başlıklı görevinin son tarihi {task.DueDate:dd.MM.yyyy}. Lütfen kontrol edin."
                );
            }
        }
    }
}
