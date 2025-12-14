using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories
{
    
    /// <summary>
    /// Handles all database operations for Tasks
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectTask?> GetByIdAsync(Guid id, Guid projectId)
        {
            return await _context.Tasks
                .Include(t => t.Project) // Include project to verify ownership
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);
        }

        public async Task<(List<ProjectTask> Tasks, int TotalCount)> GetProjectTasksAsync(
            Guid projectId,
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isCompleted = null)
        {
            var query = _context.Tasks
                .Where(t => t.ProjectId == projectId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)));
            }

            // Apply completion filter
            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            var totalCount = await query.CountAsync();

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tasks, totalCount);
        }

        public async Task<ProjectTask> AddAsync(ProjectTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(ProjectTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid projectId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId);

            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}