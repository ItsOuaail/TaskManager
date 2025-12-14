using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    /// <summary>
    /// Handles task-related endpoints
    /// Tasks are always accessed through their parent project
    /// </summary>
    [ApiController]
    [Route("api/projects/{projectId}/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tasks for a project
        /// GET /api/projects/{projectId}/tasks?page=1&pageSize=10&search=term&completed=true
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
            Guid projectId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? completed = null)
        {
            try
            {
                var userId = GetUserId();
                var tasks = await _taskService.GetProjectTasksAsync(
                    projectId, userId, page, pageSize, search, completed);

                return Ok(tasks);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
            }
        }

        /// <summary>
        /// Get a specific task
        /// GET /api/projects/{projectId}/tasks/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(Guid projectId, Guid id)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.GetTaskByIdAsync(id, projectId, userId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                return Ok(task);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the task" });
            }
        }

        /// <summary>
        /// Create a new task
        /// POST /api/projects/{projectId}/tasks
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(
            Guid projectId,
            [FromBody] CreateTaskDto dto)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.CreateTaskAsync(projectId, dto, userId);

                return CreatedAtAction(
                    nameof(GetTask),
                    new { projectId, id = task.Id },
                    task);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred while creating the task" });
            }
        }

        /// <summary>
        /// Update an existing task
        /// PUT /api/projects/{projectId}/tasks/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(
            Guid projectId,
            Guid id,
            [FromBody] UpdateTaskDto dto)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.UpdateTaskAsync(id, projectId, dto, userId);
                return Ok(task);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the task" });
            }
        }

        /// <summary>
        /// Toggle task completion status
        /// PATCH /api/projects/{projectId}/tasks/{id}/toggle
        /// </summary>
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<TaskDto>> ToggleTaskCompletion(Guid projectId, Guid id)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.ToggleTaskCompletionAsync(id, projectId, userId);
                return Ok(task);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling task completion {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the task" });
            }
        }

        /// <summary>
        /// Delete a task
        /// DELETE /api/projects/{projectId}/tasks/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(Guid projectId, Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _taskService.DeleteTaskAsync(id, projectId, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the task" });
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid user token");
            }

            return userId;
        }
    }

    /*
     * NESTED ROUTING EXPLAINED:
     * 
     * Route: [Route("api/projects/{projectId}/tasks")]
     * 
     * This creates a nested resource structure:
     * - /api/projects/123/tasks          → Get all tasks for project 123
     * - /api/projects/123/tasks/456      → Get task 456 from project 123
     * - POST /api/projects/123/tasks     → Create task in project 123
     * 
     * Why nested routes?
     * 1. RESTful design - tasks belong to projects
     * 2. Clear hierarchy in the URL
     * 3. Automatic projectId parameter
     * 4. Better security - can't access task without project context
     * 
     * HTTP VERBS:
     * - GET: Retrieve data (list or single item)
     * - POST: Create new resource
     * - PUT: Update entire resource
     * - PATCH: Partial update (toggle completion)
     * - DELETE: Remove resource
     * 
     * PATCH vs PUT:
     * - PUT: Replace entire resource (send all fields)
     * - PATCH: Modify specific fields (just toggle completed)
     * - We use PATCH for toggle because we're only changing one field
     */
}