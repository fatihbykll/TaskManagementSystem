using System.ComponentModel.DataAnnotations;
namespace TaskManagement.Application.DTOs;
public class CreateCommentDto
{
    [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Yorum 1-1000 karakter arasında olmalıdır.")]
    public string Content { get; set; } = string.Empty;
}
