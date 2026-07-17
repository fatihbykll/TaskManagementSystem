using AutoMapper;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

/// <summary>
/// Görev CRUD, sayfalı filtreleme, istatistik ve durum makinesi yönetimi; cross-user veri sızıntısını önler.
/// </summary>
public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<TaskItemDto>>> GetTasksByUserIdAsync(
        Guid userId, TaskFilterDto filter, CancellationToken cancellationToken = default)
    {
        // IQueryable pipeline: tüm filtreler DB'ye SQL olarak iletilir, belleğe yüklenmez.
        var query = _unitOfWork.Repository<TaskItem>().Query()
            .Where(t => t.UserId == userId);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.DueDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.DueDate <= filter.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            // EF Core bu ifadeyi LIKE '%term%' sorgusuna çevirir.
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term));
        }

        // Tutarlı sıralama; sayfalama için deterministik sıra zorunludur.
        query = query.OrderByDescending(t => t.CreatedAt);

        var (items, totalCount) = await _unitOfWork.Repository<TaskItem>()
            .GetPagedAsync(query, filter.PageNumber, filter.PageSize, cancellationToken);

        return ApiResponse<PagedResponse<TaskItemDto>>.SuccessResult(new PagedResponse<TaskItemDto>
        {
            Data = _mapper.Map<IEnumerable<TaskItemDto>>(items),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<ApiResponse<TaskItemDto>> GetTaskByIdAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        // UserId koşulu; başka kullanıcının görevine erişimi önler (IDOR koruması).
        var task = await _unitOfWork.Repository<TaskItem>()
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (task == null)
            return ApiResponse<TaskItemDto>.FailResult("Görev bulunamadı.");

        return ApiResponse<TaskItemDto>.SuccessResult(_mapper.Map<TaskItemDto>(task));
    }

    public async Task<ApiResponse<TaskItemDto>> CreateTaskAsync(
        Guid userId, CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        // Deaktif kullanıcının görev oluşturmasını engeller; FK ihlali öncesinde fail-fast.
        var userExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (!userExists)
            return ApiResponse<TaskItemDto>.FailResult("Geçerli bir kullanıcı bulunamadı.");

        // Cross-user category assignment'ı önler.
        if (dto.CategoryId.HasValue)
        {
            var categoryBelongsToUser = await _unitOfWork.Repository<Category>()
                .AnyAsync(c => c.Id == dto.CategoryId.Value && c.UserId == userId, cancellationToken);

            if (!categoryBelongsToUser)
                return ApiResponse<TaskItemDto>.FailResult("Belirtilen kategori bu kullanıcıya ait değil.");
        }

        // Past DueDate kabul edilmez; persistence öncesinde veri tutarsızlığı önlenir.
        if (dto.DueDate.HasValue && dto.DueDate.Value.ToUniversalTime() < DateTime.UtcNow)
            return ApiResponse<TaskItemDto>.FailResult("Bitiş tarihi geçmiş bir tarih olamaz.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            Priority = dto.Priority,
            // Initial state Pending; durum geçişleri UpdateTaskStatusAsync üzerinden yönetilir.
            Status = TaskItemStatus.Pending,
            DueDate = dto.DueDate,
            UserId = userId,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<TaskItem>().AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<TaskItemDto>.SuccessResult(
            _mapper.Map<TaskItemDto>(task), "Görev başarıyla oluşturuldu.");
    }

    public async Task<ApiResponse<TaskItemDto>> UpdateTaskAsync(
        Guid taskId, Guid userId, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<TaskItem>();

        // Ownership doğrulaması; IDOR'a karşı yeterlik ve kayıt varlığını tek sorguda kontrol eder.
        var task = await repo.FirstOrDefaultAsync(
            t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (task == null)
            return ApiResponse<TaskItemDto>.FailResult("Görev bulunamadı veya bu göreve erişim yetkiniz yok.");

        // Cross-user category assignment'ı önler.
        if (dto.CategoryId.HasValue)
        {
            var categoryBelongsToUser = await _unitOfWork.Repository<Category>()
                .AnyAsync(c => c.Id == dto.CategoryId.Value && c.UserId == userId, cancellationToken);

            if (!categoryBelongsToUser)
                return ApiResponse<TaskItemDto>.FailResult("Belirtilen kategori bu kullanıcıya ait değil.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Title)) task.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Description)) task.Description = dto.Description;
        if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
        if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
        task.CategoryId = dto.CategoryId;
        task.UpdatedAt = DateTime.UtcNow;

        repo.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<TaskItemDto>.SuccessResult(
            _mapper.Map<TaskItemDto>(task), "Görev başarıyla güncellendi.");
    }

    public async Task<ApiResponse<bool>> DeleteTaskAsync(
        Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<TaskItem>();

        // Ownership doğrulaması; IDOR'a karşı yeterlik ve kayıt varlığını tek sorguda kontrol eder.
        var task = await repo.FirstOrDefaultAsync(
            t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (task == null)
            return ApiResponse<bool>.FailResult("Görev bulunamadı veya bu göreve erişim yetkiniz yok.");

        repo.Delete(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Görev başarıyla silindi.");
    }

    public async Task<ApiResponse<TaskItemDto>> UpdateTaskStatusAsync(
        Guid taskId, Guid userId, int newStatus, CancellationToken cancellationToken = default)
    {
        // Geçersiz int cast'ini önler; dışarıdan gelen değer enum domain'i dışına çıkamaz.
        if (!Enum.IsDefined(typeof(TaskItemStatus), newStatus))
            return ApiResponse<TaskItemDto>.FailResult("Geçersiz görev durumu.");

        var repo = _unitOfWork.Repository<TaskItem>();
        var task = await repo.FirstOrDefaultAsync(
            t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (task == null)
            return ApiResponse<TaskItemDto>.FailResult("Görev bulunamadı.");

        task.Status = (TaskItemStatus)newStatus;
        task.UpdatedAt = DateTime.UtcNow;

        // CompletedAt audit alanı, status geçişiyle atomik güncellenir.
        if (task.Status == TaskItemStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;
        else
            // Status revert edildiğinde audit izi tutarsızlığı önlenir.
            task.CompletedAt = null;

        repo.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<TaskItemDto>.SuccessResult(
            _mapper.Map<TaskItemDto>(task), "Görev durumu güncellendi.");
    }

    public async Task<ApiResponse<TaskStatisticsDto>> GetStatisticsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Repository<TaskItem>()
            .FindAsync(t => t.UserId == userId, cancellationToken);

        var taskList = tasks.ToList();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return ApiResponse<TaskStatisticsDto>.SuccessResult(new TaskStatisticsDto
        {
            TotalTasks = taskList.Count,
            PendingCount = taskList.Count(t => t.Status == TaskItemStatus.Pending),
            InProgressCount = taskList.Count(t => t.Status == TaskItemStatus.InProgress),
            CompletedCount = taskList.Count(t => t.Status == TaskItemStatus.Completed),
            CancelledCount = taskList.Count(t => t.Status == TaskItemStatus.Cancelled),
            // Vadesi geçmiş: DueDate dolmuş, tamamlanmamış veya iptal edilmemiş.
            OverdueCount = taskList.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < now &&
                t.Status != TaskItemStatus.Completed &&
                t.Status != TaskItemStatus.Cancelled),
            CompletedThisMonth = taskList.Count(t =>
                t.Status == TaskItemStatus.Completed &&
                t.CompletedAt.HasValue &&
                t.CompletedAt.Value >= startOfMonth)
        });
    }

    public async Task<ApiResponse<IEnumerable<TaskItemDto>>> GetOverdueTasksAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // IQueryable pipeline: overdue filtresi DB'de çalışır.
        var query = _unitOfWork.Repository<TaskItem>().Query()
            .Where(t =>
                t.UserId == userId &&
                t.DueDate.HasValue &&
                t.DueDate.Value < now &&
                t.Status != TaskItemStatus.Completed &&
                t.Status != TaskItemStatus.Cancelled)
            .OrderBy(t => t.DueDate);

        var tasks = await _unitOfWork.Repository<TaskItem>()
            .GetPagedAsync(query, 1, 50, cancellationToken);

        return ApiResponse<IEnumerable<TaskItemDto>>.SuccessResult(
            _mapper.Map<IEnumerable<TaskItemDto>>(tasks.Items));
    }
}
