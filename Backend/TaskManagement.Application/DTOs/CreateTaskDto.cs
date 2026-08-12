using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;
namespace TaskManagement.Application.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Görev başlığı zorunludur.")]
        [StringLength(200, ErrorMessage = "Görev başlığı en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Öncelik seviyesi zorunludur.")]
        [Range(1, 5, ErrorMessage = "Öncelik seviyesi 1-5 arasında olmalıdır.")]
        public Priority Priority { get; set; } = Priority.Normal;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public DateTime? DueDate { get; set; }
        public Guid? CategoryId { get; set; }
        public RecurringFrequency RecurringFrequency { get; set; } = RecurringFrequency.None;
    }
}
