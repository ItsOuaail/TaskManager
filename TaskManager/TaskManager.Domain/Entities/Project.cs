using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Represents a project that contains tasks
    /// </summary>
    public class Project
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        // Business logic methods - these encapsulate domain rules

        /// <summary>
        /// Calculates how many tasks are completed
        /// </summary>
        public int GetCompletedTasksCount()
        {
            return Tasks.Count(t => t.IsCompleted);
        }

        /// <summary>
        /// Calculates the total number of tasks
        /// </summary>
        public int GetTotalTasksCount()
        {
            return Tasks.Count;
        }

        /// <summary>
        /// Calculates progress as a percentage (0-100)
        /// </summary>
        public double GetProgressPercentage()
        {
            if (Tasks.Count == 0)
                return 0;

            return Math.Round((double)GetCompletedTasksCount() / Tasks.Count * 100, 2);
        }
    }
}