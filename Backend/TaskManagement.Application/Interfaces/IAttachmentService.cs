using TaskManagement.Application.DTOs;
using TaskManagement.Application.Models;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Dosya eklenti yönetimi sözleşmesi.
/// FileUploadRequest ile ASP.NET Core IFormFile bağımlılığı Application'dan çıkarılır.
/// </summary>
public interface IAttachmentService
{
    Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> GetAttachmentsByTaskIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<TaskAttachmentDto>> UploadAttachmentAsync(
        Guid taskId, Guid userId, FileUploadRequest fileRequest, CancellationToken cancellationToken = default);

    /// <summary>Dosya fiziksel yolunu ve metadata'yı döner; Controller FileResult üretmekten sorumludur.</summary>
    Task<ApiResponse<(string FilePath, string ContentType, string FileName)>> GetAttachmentFileAsync(
        Guid attachmentId, Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAttachmentAsync(
        Guid attachmentId, Guid userId, CancellationToken cancellationToken = default);
}
