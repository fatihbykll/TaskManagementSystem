using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

/// <summary>
/// Görev yorum yönetimi. Yorum eklemek için görev sahipliği, silmek için yorum sahipliği doğrulanır.
/// </summary>
public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<CommentDto>>> GetCommentsByTaskIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Görev sahipliği doğrulanır; başka kullanıcının görev yorumları görüntülenemez (IDOR koruması).
        var taskExists = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (!taskExists)
            return ApiResponse<IEnumerable<CommentDto>>.FailResult("Görev bulunamadı.");

        var comments = await _unitOfWork.Repository<TaskComment>()
            .FindAsync(c => c.TaskId == taskId, cancellationToken);

        return ApiResponse<IEnumerable<CommentDto>>.SuccessResult(
            _mapper.Map<IEnumerable<CommentDto>>(comments.OrderByDescending(c => c.CreatedAt)));
    }

    public async Task<ApiResponse<CommentDto>> AddCommentAsync(
        Guid taskId, Guid userId, CreateCommentDto dto, CancellationToken cancellationToken = default)
    {
        // Yorum eklemek için görevin o kullanıcıya ait olması zorunludur.
        var taskExists = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (!taskExists)
            return ApiResponse<CommentDto>.FailResult("Görev bulunamadı.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            Comment = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<TaskComment>().AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CommentDto>.SuccessResult(
            _mapper.Map<CommentDto>(comment), "Yorum başarıyla eklendi.");
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(
        Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<TaskComment>();

        // Yalnızca yorumun sahibi silebilir; görev sahibi başkasının yorumunu silemez.
        var comment = await repo.FirstOrDefaultAsync(
            c => c.Id == commentId && c.UserId == userId, cancellationToken);

        if (comment == null)
            return ApiResponse<bool>.FailResult("Yorum bulunamadı veya silme yetkiniz yok.");

        repo.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Yorum başarıyla silindi.");
    }
}
