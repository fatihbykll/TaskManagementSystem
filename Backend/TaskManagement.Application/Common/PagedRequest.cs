using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.Common;

/// <summary>
/// Sayfalama parametreleri. Query string'den bind edilir.
/// MaxSize 50 ile sınırlandırılır; büyük dataset'lerin yanlışlıkla çekilmesini önler.
/// </summary>
public class PagedRequest
{
    private int _pageSize = 10;
    private int _pageNumber = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır.")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, 50, ErrorMessage = "Sayfa boyutu 1 ile 50 arasında olmalıdır.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value < 1 ? 1 : value;
    }
}
