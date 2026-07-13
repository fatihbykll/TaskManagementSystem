using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Görev yönetimi için servis sözleşmesi.
/// </summary>
public interface ITaskService
{
    /// <summary>Filtreleme destekli, kullanıcıya ait görevleri getirir.</summary>
    Task<ApiResponse<IEnumerable<TaskItemDto>>> GetTasksByUserIdAsync(Guid userId, TaskFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>Id'ye göre tek bir görevi getirir. Sahiplik kontrolü yapılır.</summary>
    Task<ApiResponse<TaskItemDto>> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Yeni görev oluşturur. Belirtilen kategorinin kullanıcıya ait olup olmadığı doğrulanır.</summary>
    Task<ApiResponse<TaskItemDto>> CreateTaskAsync(Guid userId, CreateTaskDto dto, CancellationToken cancellationToken = default);

    /// <summary>Görevi günceller. Başlık, açıklama, öncelik, durum ve kategori değiştirilebilir.</summary>
    Task<ApiResponse<TaskItemDto>> UpdateTaskAsync(Guid taskId, Guid userId, UpdateTaskDto dto, CancellationToken cancellationToken = default);

    /// <summary>Görevi siler. Sahiplik kontrolü yapılır.</summary>
    Task<ApiResponse<bool>> DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Görevin durumunu günceller. Completed yapılınca CompletedAt otomatik set edilir.</summary>
    Task<ApiResponse<TaskItemDto>> UpdateTaskStatusAsync(Guid taskId, Guid userId, int newStatus, CancellationToken cancellationToken = default);
}
