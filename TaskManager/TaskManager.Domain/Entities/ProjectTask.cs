using System;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Represents a task within a project
    /// Note: Named "ProjectTask" instead of "Task" to avoid conflicts with System.Threading.Tasks.Task
    /// </summary>
    public class ProjectTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Foreign key
        public Guid ProjectId { get; set; }

        // Navigation property
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// Marks the task as completed
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the task as incomplete
        /// </summary>
        public void MarkAsIncomplete()
        {
            IsCompleted = false;
            CompletedAt = null;
        }
    }
}