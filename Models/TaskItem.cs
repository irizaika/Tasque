using System.ComponentModel.DataAnnotations;

namespace Tasque.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? Group { get; set; }

        public string AssignedTo { get; set; } // user email

        public TaskStatus Status { get; set; } // Enum: ToDo, InProgress, Done, Closed

        public TaskPriority Priority { get; set; } // Enum: Low, Medium, High

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public string CreatedBy { get; set; } // user email

        public string TenantId { get; set; } = "";
    }
}
