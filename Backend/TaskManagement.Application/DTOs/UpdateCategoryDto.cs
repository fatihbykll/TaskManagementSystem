using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class UpdateCategoryDto
    {
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Renk kodu #XXXXXX formatında olmalıdır.")]
        public string? Color { get; set; }
    }
}
