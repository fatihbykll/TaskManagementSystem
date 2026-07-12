using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Normal;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public Category? Category { get; set; }
        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
