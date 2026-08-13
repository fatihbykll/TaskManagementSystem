namespace TaskManagement.Application.Interfaces;
public record DailyReportDto(
    DateTime Date,
    int CreatedTasks,
    int CompletedTasks,
    int OverdueTasks,
    IEnumerable<TopUserDto> TopUsers
);
public record TopUserDto(string Username, int CompletedCount);
public record ProductivityDto(
    int TotalTasks,
    int CompletedTasks,
    double CompletionRate,
    int StreakDays,
    string PerformanceLabel
);
public interface IReportService
{
    Task<DailyReportDto> GetDailyReportAsync(CancellationToken ct = default);
    Task<ProductivityDto> GetUserProductivityAsync(Guid userId, CancellationToken ct = default);
}
