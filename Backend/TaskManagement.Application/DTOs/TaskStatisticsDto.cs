namespace TaskManagement.Application.DTOs;
/// <summary>
/// Görev istatistikleri özeti. Dashboard ve raporlama için kullanılır.
/// </summary>
public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    /// <summary>DueDate geçmiş, tamamlanmamış veya iptal edilmemiş görevler.</summary>
    public int OverdueCount { get; set; }
    public int CompletedThisMonth { get; set; }
}
