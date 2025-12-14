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
    /// Service layer for task business logic
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
        }

        /// <summary>
        /// Gets all tasks for a project with pagination and filtering
        /// </summary>
        public async Task<PagedResult<TaskDto>> GetProjectTasksAsync(
            Guid projectId,
            Guid userId,
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isCompleted = null)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // Get tasks
            var (tasks, totalCount) = await _taskRepository.GetProjectTasksAsync(
                projectId, page, pageSize, searchTerm, isCompleted);

            // Map to DTOs
            var taskDtos = tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                ProjectId = t.ProjectId
            }).ToList();

            return new PagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Gets a single task by ID
        /// </summary>
        public async Task<TaskDto?> GetTaskByIdAsync(Guid id, Guid projectId, Guid userId)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            var task = await _taskRepository.GetByIdAsync(id, projectId);

            if (task == null)
            {
                return null;
            }

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                ProjectId = task.ProjectId
            };
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public async Task<TaskDto> CreateTaskAsync(Guid projectId, CreateTaskDto dto, Guid userId)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            // Validate
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ValidationException("Task title is required");
            }

            // Create domain entity
            var task = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                DueDate = dto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                ProjectId = projectId
            };

            // Save to database
            await _taskRepository.AddAsync(task);

            // Return DTO
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                ProjectId = task.ProjectId
            };
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        public async Task<TaskDto> UpdateTaskAsync(
            Guid id,
            Guid projectId,
            UpdateTaskDto dto,
            Guid userId)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            // Get existing task
            var task = await _taskRepository.GetByIdAsync(id, projectId);
            if (task == null)
            {
                throw new NotFoundException("Task", id);
            }

            // Validate
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ValidationException("Task title is required");
            }

            // Update properties
            task.Title = dto.Title.Trim();
            task.Description = dto.Description?.Trim();
            task.DueDate = dto.DueDate;

            // Save changes
            await _taskRepository.UpdateAsync(task);

            // Return updated DTO
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                ProjectId = task.ProjectId
            };
        }

        /// <summary>
        /// Toggles task completion status
        /// </summary>
        public async Task<TaskDto> ToggleTaskCompletionAsync(
            Guid id,
            Guid projectId,
            Guid userId)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            // Get task
            var task = await _taskRepository.GetByIdAsync(id, projectId);
            if (task == null)
            {
                throw new NotFoundException("Task", id);
            }

            // Toggle completion using domain method
            if (task.IsCompleted)
            {
                task.MarkAsIncomplete();
            }
            else
            {
                task.MarkAsCompleted();
            }

            // Save changes
            await _taskRepository.UpdateAsync(task);

            // Return updated DTO
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                ProjectId = task.ProjectId
            };
        }

        /// <summary>
        /// Deletes a task
        /// </summary>
        public async Task DeleteTaskAsync(Guid id, Guid projectId, Guid userId)
        {
            // Verify user owns the project
            var project = await _projectRepository.GetByIdAsync(projectId, userId);
            if (project == null)
            {
                throw new NotFoundException("Project", projectId);
            }

            // Verify task exists
            var task = await _taskRepository.GetByIdAsync(id, projectId);
            if (task == null)
            {
                throw new NotFoundException("Task", id);
            }

            // Delete
            await _taskRepository.DeleteAsync(id, projectId);
        }
    }

    /*
     * KEY CONCEPTS IN THIS SERVICE:
     * 
     * 1. AUTHORIZATION:
     *    - Always verify user owns the project before allowing task operations
     *    - This prevents users from accessing/modifying other users' tasks
     * 
     * 2. VALIDATION:
     *    - Check required fields (title)
     *    - Validate pagination parameters
     *    - Throw meaningful exceptions
     * 
     * 3. BUSINESS LOGIC:
     *    - Toggle completion uses domain method (MarkAsCompleted/MarkAsIncomplete)
     *    - This ensures business rules are enforced (like setting CompletedAt)
     * 
     * 4. FILTERING & SEARCH:
     *    - Support searching by title/description
     *    - Support filtering by completion status
     *    - These make the API more useful for users
     */
}