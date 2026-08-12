using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.Infrastructure.Jobs;
public class RecurringTaskGeneratorJob : IRecurringTaskGeneratorJob
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<RecurringTaskGeneratorJob> _logger;
    public RecurringTaskGeneratorJob(ApplicationDbContext db, ILogger<RecurringTaskGeneratorJob> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Hangfire Job Başladı: RecurringTaskGeneratorJob");
        var now = DateTime.UtcNow;
        // NextRunDate'i gelmiş (veya geçmiş) ve tekrarlama frekansı olan (None olmayan) görevleri al
        var tasksToDuplicate = await _db.Tasks
            .Where(t => t.RecurringFrequency != RecurringFrequency.None &&
                        t.NextRunDate != null &&
                        t.NextRunDate <= now)
            .ToListAsync(ct);
        if (!tasksToDuplicate.Any())
        {
            _logger.LogInformation("Tekrarlanacak görev bulunamadı.");
            return;
        }
        var newTasks = new List<TaskItem>();
        foreach (var task in tasksToDuplicate)
        {
            // Yeni bir kopya oluştur
            var newTask = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Status = TaskItemStatus.Pending, // Yeni görev beklemede başlar
                UserId = task.UserId,
                CategoryId = task.CategoryId,
                CreatedAt = now,
                UpdatedAt = now,
                RecurringFrequency = RecurringFrequency.None, // Kopya görev tekrar etmez, asıl görev tekrar eder
                ParentTaskId = task.Id, // Hangi görevden koptuğunu işaretle
            };
            // DueDate'i de ileriye taşıyalım (Eğer orijinalinde varsa)
            if (task.DueDate.HasValue)
            {
                var diff = task.DueDate.Value - task.CreatedAt;
                newTask.DueDate = now.Add(diff); // Orijinal süre kadar bir deadline ver
            }
            newTasks.Add(newTask);
            // Ana (Parent) görevin NextRunDate'ini bir sonraki periyoda ötele
            switch (task.RecurringFrequency)
            {
                case RecurringFrequency.Daily:
                    task.NextRunDate = task.NextRunDate.Value.AddDays(1);
                    break;
                case RecurringFrequency.Weekly:
                    task.NextRunDate = task.NextRunDate.Value.AddDays(7);
                    break;
                case RecurringFrequency.Monthly:
                    task.NextRunDate = task.NextRunDate.Value.AddMonths(1);
                    break;
            }
            
            task.UpdatedAt = now;
        }
        await _db.Tasks.AddRangeAsync(newTasks, ct);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation($"Hangfire Job Bitti: Toplam {newTasks.Count} yeni tekrarlayan görev oluşturuldu.");
    }
}
