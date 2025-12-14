using System;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for authentication operations (login, register)
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Step 1: Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            // Step 2: Verify password
            // BCrypt.Net.BCrypt.Verify compares the plain password with the hashed one
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            // Step 3: Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Step 4: Return login response
            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id
            };
        }

        /// <summary>
        /// Registers a new user and returns a JWT token
        /// </summary>
        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            // Step 1: Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new DuplicateException("Email already registered");
            }

            // Step 2: Hash the password
            // NEVER store plain text passwords!
            // BCrypt is a strong hashing algorithm designed for passwords
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Step 3: Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower(), // Store email in lowercase for consistency
                Name = request.Name,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Step 4: Save to database
            await _userRepository.AddAsync(user);

            // Step 5: Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Step 6: Return login response
            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id
            };
        }
    }

    /*
     * PASSWORD SECURITY EXPLAINED:
     * 
     * WHY HASH PASSWORDS?
     * - If database is compromised, attackers can't see actual passwords
     * - Each hash is unique even for same password (due to salt)
     * - Hashing is one-way (can't reverse it to get original password)
     * 
     * BCRYPT ALGORITHM:
     * - Industry standard for password hashing
     * - Automatically includes a "salt" (random data added to password)
     * - Computationally expensive (slows down brute force attacks)
     * - Adaptive (can increase difficulty over time)
     * 
     * HOW IT WORKS:
     * 1. Registration: BCrypt.HashPassword("password123") → "$2a$11$xQdvN3j0..."
     * 2. Login: BCrypt.Verify("password123", "$2a$11$xQdvN3j0...") → true/false
     * 
     * NEVER:
     * - Store plain text passwords
     * - Use weak hashing (MD5, SHA1)
     * - Hash without salt
     * - Implement your own crypto
     */
}