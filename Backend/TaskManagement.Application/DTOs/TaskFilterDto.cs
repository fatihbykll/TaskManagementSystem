using TaskManagement.Application.Common;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Görev listeleme için filtre + sayfalama parametreleri.
/// PagedRequest'ten miras alınır; tüm query string parametreleri tek nesnede toplanır.
/// </summary>
public class TaskFilterDto : PagedRequest
{
    public TaskItemStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Title ve Description'da case-insensitive LIKE araması için.
    public string? SearchTerm { get; set; }
}
