using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Görev CRUD, filtreleme, sayfalama, istatistik ve durum geçiş sözleşmesi.
/// </summary>
public interface ITaskService
{
    /// <summary>Filtrelenmiş ve sayfalanmış görev listesi. Tüm filtreler DB'de çalışır.</summary>
    Task<ApiResponse<PagedResponse<TaskItemDto>>> GetTasksByUserIdAsync(
        Guid userId, TaskFilterDto filter, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskItemDto>> GetTaskByIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskItemDto>> CreateTaskAsync(
        Guid userId, CreateTaskDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskItemDto>> UpdateTaskAsync(
        Guid taskId, Guid userId, UpdateTaskDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteTaskAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskItemDto>> UpdateTaskStatusAsync(
        Guid taskId, Guid userId, int newStatus, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcıya ait görev istatistiklerini döner. Durum dağılımı ve overdue bilgisi içerir.</summary>
    Task<ApiResponse<TaskStatisticsDto>> GetStatisticsAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>DueDate geçmiş ve Completed/Cancelled olmayan görevleri döner.</summary>
    Task<ApiResponse<IEnumerable<TaskItemDto>>> GetOverdueTasksAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
