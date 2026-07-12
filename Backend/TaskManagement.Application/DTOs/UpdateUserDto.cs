using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }
    }
}
