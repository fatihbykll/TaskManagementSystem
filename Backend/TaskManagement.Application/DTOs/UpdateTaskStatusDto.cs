using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Görev durum geçişi için request zarfı.
/// Durum değişikliği ayrı endpoint'te izole edilir; partial update anti-pattern'ini önler.
/// </summary>
public class UpdateTaskStatusDto
{
    [Required(ErrorMessage = "Durum alanı zorunludur.")]
    [Range(0, 3, ErrorMessage = "Durum değeri 0 (Pending) ile 3 (Cancelled) arasında olmalıdır.")]
    public int Status { get; set; }
}
