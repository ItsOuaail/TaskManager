using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Repository for Task data access
    /// </summary>
    public interface ITaskRepository
    {
        Task<ProjectTask?> GetByIdAsync(Guid id, Guid projectId);
        Task<(List<ProjectTask> Tasks, int TotalCount)> GetProjectTasksAsync(
            Guid projectId,
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isCompleted = null);
        Task<ProjectTask> AddAsync(ProjectTask task);
        Task UpdateAsync(ProjectTask task);
        Task DeleteAsync(Guid id, Guid projectId);
    }
}
