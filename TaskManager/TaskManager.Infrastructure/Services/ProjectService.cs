using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Service layer for project business logic
    /// This sits between the API controllers and the data repositories
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        /// <summary>
        /// Gets all projects for a user with pagination
        /// </summary>
        public async Task<PagedResult<ProjectDto>> GetUserProjectsAsync(
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // Get data from repository
            var (projects, totalCount) = await _projectRepository.GetUserProjectsAsync(
                userId, page, pageSize, searchTerm);

            // Map domain entities to DTOs
            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                TotalTasks = p.GetTotalTasksCount(),
                CompletedTasks = p.GetCompletedTasksCount(),
                ProgressPercentage = p.GetProgressPercentage()
            }).ToList();

            // Return paginated result
            return new PagedResult<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Gets a single project with all its tasks
        /// </summary>
        public async Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(id, userId);

            if (project == null)
            {
                return null;
            }

            // Map to detailed DTO including all tasks
            return new ProjectDetailDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TotalTasks = project.GetTotalTasksCount(),
                CompletedTasks = project.GetCompletedTasksCount(),
                ProgressPercentage = project.GetProgressPercentage(),
                Tasks = project.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    ProjectId = t.ProjectId
                }).OrderByDescending(t => t.CreatedAt).ToList()
            };
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid userId)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ValidationException("Project title is required");
            }

            // Create domain entity
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await _projectRepository.AddAsync(project);

            // Return DTO
            return new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TotalTasks = 0,
                CompletedTasks = 0,
                ProgressPercentage = 0
            };
        }

        /// <summary>
        /// Updates an existing project
        /// </summary>
        public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid userId)
        {
            // Get existing project
            var project = await _projectRepository.GetByIdAsync(id, userId);

            if (project == null)
            {
                throw new NotFoundException("Project", id);
            }

            // Validate
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ValidationException("Project title is required");
            }

            // Update properties
            project.Title = dto.Title.Trim();
            project.Description = dto.Description?.Trim();

            // Save changes
            await _projectRepository.UpdateAsync(project);

            // Return updated DTO
            return new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TotalTasks = project.GetTotalTasksCount(),
                CompletedTasks = project.GetCompletedTasksCount(),
                ProgressPercentage = project.GetProgressPercentage()
            };
        }

        /// <summary>
        /// Deletes a project and all its tasks
        /// </summary>
        public async Task DeleteProjectAsync(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(id, userId);

            if (project == null)
            {
                throw new NotFoundException("Project", id);
            }

            await _projectRepository.DeleteAsync(id, userId);
        }

        /// <summary>
        /// Gets progress information for a project
        /// </summary>
        public async Task<ProjectProgressDto> GetProjectProgressAsync(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(id, userId);

            if (project == null)
            {
                throw new NotFoundException("Project", id);
            }

            return new ProjectProgressDto
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                TotalTasks = project.GetTotalTasksCount(),
                CompletedTasks = project.GetCompletedTasksCount(),
                ProgressPercentage = project.GetProgressPercentage()
            };
        }
    }

    /*
     * SERVICE LAYER RESPONSIBILITIES:
     * 
     * 1. BUSINESS LOGIC:
     *    - Validation (title required, etc.)
     *    - Business rules (calculate progress, etc.)
     *    - Orchestration (coordinate multiple repositories)
     * 
     * 2. DATA TRANSFORMATION:
     *    - Convert domain entities to DTOs
     *    - Convert DTOs to domain entities
     *    - This keeps domain objects separate from API concerns
     * 
     * 3. ERROR HANDLING:
     *    - Throw meaningful exceptions
     *    - Validate input before database operations
     * 
     * WHY NOT PUT THIS IN CONTROLLERS?
     * - Controllers should be thin (just route requests)
     * - Business logic should be testable without HTTP
     * - Services can be reused (API, background jobs, etc.)
     * 
     * WHY NOT PUT THIS IN REPOSITORIES?
     * - Repositories should only handle data access
     * - Business logic doesn't belong in data layer
     * - Separation of concerns
     */
}