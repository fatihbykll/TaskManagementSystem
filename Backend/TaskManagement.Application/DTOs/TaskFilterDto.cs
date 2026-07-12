using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs
{
    public class TaskFilterDto
    {
        public TaskItemStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
