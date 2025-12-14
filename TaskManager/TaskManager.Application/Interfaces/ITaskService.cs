using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Service for task operations
    /// </summary>
    public interface ITaskService
    {
        Task<PagedResult<TaskDto>> GetProjectTasksAsync(
            Guid projectId,
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isCompleted = null);
        Task<TaskDto?> GetTaskByIdAsync(Guid id, Guid projectId, Guid userId);
        Task<TaskDto> CreateTaskAsync(Guid projectId, CreateTaskDto dto, Guid userId);
        Task<TaskDto> UpdateTaskAsync(Guid id, Guid projectId, UpdateTaskDto dto, Guid userId);
        Task<TaskDto> ToggleTaskCompletionAsync(Guid id, Guid projectId, Guid userId);
        Task DeleteTaskAsync(Guid id, Guid projectId, Guid userId);
    }
}
