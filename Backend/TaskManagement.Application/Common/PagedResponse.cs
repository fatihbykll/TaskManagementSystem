namespace TaskManagement.Application.Common;

/// <summary>
/// Sayfalı sorgu sonucu zarfı. İstemcinin sonraki sayfa isteği yapıp yapmayacağına
/// karar verebilmesi için metadata taşır.
/// </summary>
public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
