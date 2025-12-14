using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Repository for Project data access
    /// </summary>
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id, Guid userId);
        Task<(List<Project> Projects, int TotalCount)> GetUserProjectsAsync(
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null);
        Task<Project> AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Guid id, Guid userId);
    }
}
