using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Models;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
/// Dosya eklenti yönetimi. Whitelist tabanlı uzantı/MIME kontrolü ve
/// GUID yeniden adlandırma ile path traversal saldırıları önlenir.
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    // Güvenlik açığı oluşturabilecek çalıştırılabilir uzantılar dahil edilmez.
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx", ".txt"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain"
    };

    public AttachmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// wwwroot/uploads/attachments dizinini döner; dizin yoksa oluşturur.
    /// IWebHostEnvironment yerine GetCurrentDirectory() kullanılır; Infrastructure'da ASP.NET Core host bağımlılığı olmaz.
    /// </summary>
    private static string GetUploadDirectory()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "attachments");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public async Task<ApiResponse<IEnumerable<TaskAttachmentDto>>> GetAttachmentsByTaskIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Görev sahipliği doğrulanır; başka kullanıcının eklerini listelemeyi önler (IDOR koruması).
        var taskExists = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (!taskExists)
            return ApiResponse<IEnumerable<TaskAttachmentDto>>.FailResult("Görev bulunamadı.");

        var attachments = await _unitOfWork.Repository<TaskAttachment>()
            .FindAsync(a => a.TaskId == taskId, cancellationToken);

        return ApiResponse<IEnumerable<TaskAttachmentDto>>.SuccessResult(
            _mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments));
    }

    public async Task<ApiResponse<TaskAttachmentDto>> UploadAttachmentAsync(
        Guid taskId, Guid userId, FileUploadRequest fileRequest, CancellationToken cancellationToken = default)
    {
        var taskExists = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (!taskExists)
            return ApiResponse<TaskAttachmentDto>.FailResult("Görev bulunamadı.");

        if (fileRequest.Size == 0)
            return ApiResponse<TaskAttachmentDto>.FailResult("Dosya boş olamaz.");

        if (fileRequest.Size > MaxFileSizeBytes)
            return ApiResponse<TaskAttachmentDto>.FailResult("Dosya boyutu 10 MB'ı aşamaz.");

        var extension = Path.GetExtension(fileRequest.FileName);
        if (!AllowedExtensions.Contains(extension))
            return ApiResponse<TaskAttachmentDto>.FailResult(
                $"Desteklenmeyen dosya türü. İzin verilenler: {string.Join(", ", AllowedExtensions)}");

        if (!AllowedMimeTypes.Contains(fileRequest.ContentType))
            return ApiResponse<TaskAttachmentDto>.FailResult("Geçersiz MIME türü.");

        // GUID ile yeniden adlandırma; path traversal ve dosya adı çakışmalarını önler.
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(GetUploadDirectory(), storedFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await fileRequest.Content.CopyToAsync(stream, cancellationToken);

        var attachment = new TaskAttachment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            FileName = fileRequest.FileName,
            FilePath = filePath,
            FileSize = fileRequest.Size,
            ContentType = fileRequest.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<TaskAttachment>().AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<TaskAttachmentDto>.SuccessResult(
            _mapper.Map<TaskAttachmentDto>(attachment), "Dosya başarıyla yüklendi.");
    }

    public async Task<ApiResponse<(string FilePath, string ContentType, string FileName)>> GetAttachmentFileAsync(
        Guid attachmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var attachment = await _unitOfWork.Repository<TaskAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId, cancellationToken);

        if (attachment == null)
            return ApiResponse<(string, string, string)>.FailResult("Ek bulunamadı.");

        // Dosyanın bağlı olduğu görevin sahibi doğrulanır.
        var taskBelongsToUser = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == attachment.TaskId && t.UserId == userId, cancellationToken);

        if (!taskBelongsToUser)
            return ApiResponse<(string, string, string)>.FailResult("Bu dosyaya erişim yetkiniz yok.");

        if (!File.Exists(attachment.FilePath))
            return ApiResponse<(string, string, string)>.FailResult("Dosya fiziksel olarak bulunamadı.");

        return ApiResponse<(string, string, string)>.SuccessResult(
            (attachment.FilePath, attachment.ContentType, attachment.FileName));
    }

    public async Task<ApiResponse<bool>> DeleteAttachmentAsync(
        Guid attachmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var attachment = await _unitOfWork.Repository<TaskAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId, cancellationToken);

        if (attachment == null)
            return ApiResponse<bool>.FailResult("Ek bulunamadı.");

        var taskBelongsToUser = await _unitOfWork.Repository<TaskItem>()
            .AnyAsync(t => t.Id == attachment.TaskId && t.UserId == userId, cancellationToken);

        if (!taskBelongsToUser)
            return ApiResponse<bool>.FailResult("Bu dosyayı silme yetkiniz yok.");

        // DB kaydı önce silinir; fiziksel silme başarısız olsa bile veri bütünlüğü korunur.
        _unitOfWork.Repository<TaskAttachment>().Delete(attachment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (File.Exists(attachment.FilePath))
            File.Delete(attachment.FilePath);

        return ApiResponse<bool>.SuccessResult(true, "Dosya başarıyla silindi.");
    }
}
