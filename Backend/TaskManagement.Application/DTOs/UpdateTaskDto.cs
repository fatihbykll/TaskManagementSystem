using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;
namespace TaskManagement.Application.DTOs
{
    public class UpdateTaskDto
    {
        [StringLength(200, ErrorMessage = "Görev başlığı en fazla 200 karakter olabilir.")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Range(1, 5, ErrorMessage = "Öncelik seviyesi 1-5 arasında olmalıdır.")]
        public Priority? Priority { get; set; }
        public TaskItemStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? CategoryId { get; set; }
        public RecurringFrequency? RecurringFrequency { get; set; }
    }
}
