using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Görev yorum yönetimi sözleşmesi.
/// Yorum ekleme ve silmede görev sahipliği doğrulaması zorunludur.
/// </summary>
public interface ICommentService
{
    Task<ApiResponse<IEnumerable<CommentDto>>> GetCommentsByTaskIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CommentDto>> AddCommentAsync(
        Guid taskId, Guid userId, CreateCommentDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteCommentAsync(
        Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
