using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    /// <summary>
    /// Handles project-related endpoints
    /// All endpoints require authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require JWT authentication
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects for the authenticated user
        /// GET /api/projects?page=1&pageSize=10&search=term
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProjectDto>>> GetProjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                // Extract user ID from JWT token
                var userId = GetUserId();

                var projects = await _projectService.GetUserProjectsAsync(userId, page, pageSize, search);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                return StatusCode(500, new { message = "An error occurred while retrieving projects" });
            }
        }

        /// <summary>
        /// Get a specific project by ID
        /// GET /api/projects/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDetailDto>> GetProject(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var project = await _projectService.GetProjectByIdAsync(id, userId);

                if (project == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the project" });
            }
        }

        /// <summary>
        /// Create a new project
        /// POST /api/projects
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var userId = GetUserId();
                var project = await _projectService.CreateProjectAsync(dto, userId);

                // Return 201 Created with Location header
                return CreatedAtAction(
                    nameof(GetProject),
                    new { id = project.Id },
                    project);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { message = "An error occurred while creating the project" });
            }
        }

        /// <summary>
        /// Update an existing project
        /// PUT /api/projects/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var userId = GetUserId();
                var project = await _projectService.UpdateProjectAsync(id, dto, userId);
                return Ok(project);
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
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the project" });
            }
        }

        /// <summary>
        /// Delete a project
        /// DELETE /api/projects/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _projectService.DeleteProjectAsync(id, userId);
                return NoContent(); // 204 No Content
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the project" });
            }
        }

        /// <summary>
        /// Get project progress
        /// GET /api/projects/{id}/progress
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<ActionResult<ProjectProgressDto>> GetProjectProgress(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var progress = await _projectService.GetProjectProgressAsync(id, userId);
                return Ok(progress);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project progress {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving project progress" });
            }
        }

        /// <summary>
        /// Helper method to extract user ID from JWT token claims
        /// </summary>
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
     * KEY CONCEPTS:
     * 
     * 1. [Authorize] ATTRIBUTE:
     *    - Requires valid JWT token for all endpoints
     *    - Automatically returns 401 if no/invalid token
     * 
     * 2. EXTRACTING USER FROM TOKEN:
     *    - User.FindFirstValue(ClaimTypes.NameIdentifier) gets user ID from JWT
     *    - This was added to token in TokenService.GenerateToken()
     * 
     * 3. QUERY PARAMETERS:
     *    - [FromQuery] binds from URL: ?page=1&pageSize=10
     *    - Default values: page = 1, pageSize = 10
     * 
     * 4. ROUTE PARAMETERS:
     *    - {id} in route matches method parameter: GetProject(Guid id)
     * 
     * 5. HTTP STATUS CODES:
     *    - 200 OK: Success with data
     *    - 201 Created: Resource created successfully
     *    - 204 No Content: Success without data (delete)
     *    - 400 Bad Request: Validation error
     *    - 401 Unauthorized: Not authenticated
     *    - 404 Not Found: Resource doesn't exist
     *    - 500 Internal Server Error: Server error
     * 
     * 6. CreatedAtAction():
     *    - Returns 201 with Location header
     *    - Location: /api/projects/{new-id}
     *    - Client can use this to fetch the new resource
     */
}