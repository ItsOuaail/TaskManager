using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers
{
    /// <summary>
    /// Handles authentication endpoints (login, register)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login endpoint - returns JWT token
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous] // Don't require authentication for login
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Register endpoint - creates new user and returns JWT token
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (DuplicateException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }
    }

    /*
     * CONTROLLER EXPLAINED:
     * 
     * 1. [ApiController] ATTRIBUTE:
     *    - Automatic model validation
     *    - Automatic 400 responses for invalid models
     *    - Binds parameters from request body automatically
     * 
     * 2. [Route("api/[controller]")] ATTRIBUTE:
     *    - [controller] is replaced with "Auth"
     *    - So this controller handles: /api/auth/*
     * 
     * 3. [HttpPost("login")] ATTRIBUTE:
     *    - This method handles POST requests to /api/auth/login
     * 
     * 4. [AllowAnonymous] ATTRIBUTE:
     *    - Overrides global [Authorize] requirement
     *    - Login/register don't require authentication
     * 
     * 5. ActionResult<T>:
     *    - Return type that can be either data (T) or status code
     *    - Ok() returns 200, BadRequest() returns 400, etc.
     * 
     * 6. [FromBody]:
     *    - Tells ASP.NET to deserialize JSON body into the parameter
     *    - Request body: {"email": "...", "password": "..."}
     * 
     * 7. ERROR HANDLING:
     *    - Catch specific exceptions (UnauthorizedException, DuplicateException)
     *    - Return appropriate HTTP status codes
     *    - Log errors for debugging
     */
}