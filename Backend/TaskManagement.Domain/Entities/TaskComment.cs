namespace TaskManagement.Domain.Entities
{
    public class TaskComment
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public TaskItem TaskItem { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
