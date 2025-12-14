using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories
{
    
    /// <summary>
    /// Handles all database operations for Projects
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(Guid id, Guid userId)
        {
            // Include() performs a SQL JOIN to load related tasks
            // This is called "eager loading"
            return await _context.Projects
                .Include(p => p.Tasks) // Load all tasks with the project
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<(List<Project> Projects, int TotalCount)> GetUserProjectsAsync(
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null)
        {
            // Start with base query
            var query = _context.Projects
                .Include(p => p.Tasks) // Load tasks for progress calculation
                .Where(p => p.UserId == userId);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    (p.Description != null && p.Description.Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var projects = await query
                .OrderByDescending(p => p.CreatedAt) // Newest first
                .Skip((page - 1) * pageSize) // Skip previous pages
                .Take(pageSize) // Take only current page
                .ToListAsync();

            return (projects, totalCount);
        }

        public async Task<Project> AddAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync(); // This commits the transaction
            return project;
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }
    }

    
}