using System;
using System.Collections.Generic;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation property - EF Core will use this to load related projects
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}