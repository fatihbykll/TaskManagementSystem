using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.Infrastructure.Services;
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;
    public ReportService(ApplicationDbContext db) => _db = db;
    public async Task<DailyReportDto> GetDailyReportAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var now = DateTime.UtcNow;
        var createdToday = await _db.Tasks
            .CountAsync(t => t.CreatedAt >= today && t.CreatedAt < tomorrow, ct);
        var completedToday = await _db.Tasks
            .CountAsync(t => t.Status == TaskItemStatus.Completed &&
                            t.CompletedAt >= today && t.CompletedAt < tomorrow, ct);
        var overdueCount = await _db.Tasks
            .CountAsync(t => t.DueDate < now &&
                            t.Status != TaskItemStatus.Completed &&
                            t.Status != TaskItemStatus.Cancelled, ct);
        var topUsers = await _db.Tasks
            .Where(t => t.Status == TaskItemStatus.Completed &&
                       t.CompletedAt >= today && t.CompletedAt < tomorrow)
            .GroupBy(t => t.User.Username)
            .Select(g => new TopUserDto(g.Key, g.Count()))
            .OrderByDescending(u => u.CompletedCount)
            .Take(5)
            .ToListAsync(ct);
        return new DailyReportDto(today, createdToday, completedToday, overdueCount, topUsers);
    }
    public async Task<ProductivityDto> GetUserProductivityAsync(Guid userId, CancellationToken ct = default)
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync(ct);
        var total = tasks.Count;
        var completed = tasks.Count(t => t.Status == TaskItemStatus.Completed);
        var rate = total == 0 ? 0.0 : Math.Round((double)completed / total * 100, 1);
        // Üst üste kaç gün görev tamamlanmış (streak)
        var completedDates = tasks
            .Where(t => t.CompletedAt.HasValue)
            .Select(t => t.CompletedAt!.Value.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();
        var streak = 0;
        var checkDate = DateTime.UtcNow.Date;
        foreach (var date in completedDates)
        {
            if (date == checkDate) { streak++; checkDate = checkDate.AddDays(-1); }
            else break;
        }
        var label = rate switch
        {
            >= 80 => "🏆 Yüksek Performans",
            >= 50 => "👍 Orta Performans",
            >= 20 => "📈 Gelişiyor",
            _     => "🚀 Başlangıç"
        };
        return new ProductivityDto(total, completed, rate, streak, label);
    }
}
