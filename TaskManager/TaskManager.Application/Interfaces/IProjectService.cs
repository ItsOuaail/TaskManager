using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Service for project operations
    /// </summary>
    public interface IProjectService
    {
        Task<PagedResult<ProjectDto>> GetUserProjectsAsync(
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null);
        Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id, Guid userId);
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid userId);
        Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid userId);
        Task DeleteProjectAsync(Guid id, Guid userId);
        Task<ProjectProgressDto> GetProjectProgressAsync(Guid id, Guid userId);
    }
}
