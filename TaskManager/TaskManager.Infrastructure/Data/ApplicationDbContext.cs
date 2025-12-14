using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data
{
    /// <summary>
    /// Database context for the application
    /// This class is the bridge between your C# code and the PostgreSQL database
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets represent tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }

        /// <summary>
        /// Configures the database schema using Fluent API
        /// This is where we define relationships, constraints, and indexes
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== User Configuration ====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                // Email must be unique and is required
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .IsRequired();
            });

            // ==================== Project Configuration ====================
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Description)
                    .HasMaxLength(1000);

                entity.Property(p => p.CreatedAt)
                    .IsRequired();

                // Define relationship: One User has many Projects
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Projects)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete all their projects

                // Create index on UserId for faster queries
                entity.HasIndex(p => p.UserId);
            });

            // ==================== Task Configuration ====================
            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                entity.Property(t => t.IsCompleted)
                    .IsRequired();

                entity.Property(t => t.CreatedAt)
                    .IsRequired();

                // Define relationship: One Project has many Tasks
                entity.HasOne(t => t.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade); // If project is deleted, delete all its tasks

                // Create index on ProjectId for faster queries
                entity.HasIndex(t => t.ProjectId);

                // Create index on IsCompleted for faster filtering
                entity.HasIndex(t => t.IsCompleted);
            });

            // ==================== Seed Data (Optional) ====================
            // This creates default users for testing
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = userId,
                    Email = "admin@test.com",
                    Name = "Admin User",
                    // Password: "password123" (hashed with BCrypt)
                    PasswordHash = "$2a$11$xQdvN3j0Zy0KxqCHNlFZ8Od.0pZqYZGMVh6OyJ7rqhVJ4kZ4sQWbS",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Email = "user@test.com",
                    Name = "Test User",
                    PasswordHash = "$2a$11$xQdvN3j0Zy0KxqCHNlFZ8Od.0pZqYZGMVh6OyJ7rqhVJ4kZ4sQWbS",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}