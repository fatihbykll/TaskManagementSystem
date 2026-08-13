using TaskManagement.Domain.Enums;
namespace TaskManagement.Application.DTOs
{
    public class TaskItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public TaskItemStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public RecurringFrequency RecurringFrequency { get; set; }
        public DateTime? NextRunDate { get; set; }
        public Guid? ParentTaskId { get; set; }
    }
}
