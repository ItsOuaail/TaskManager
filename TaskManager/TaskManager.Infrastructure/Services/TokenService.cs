using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for generating JWT tokens
    /// JWT = JSON Web Token - a secure way to transmit information between parties
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT token for a user
        /// The token contains user information (claims) and is digitally signed
        /// </summary>
        public string GenerateToken(User user)
        {
            // Step 1: Create claims (information stored in the token)
            // Claims are key-value pairs that describe the user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID
                new Claim(ClaimTypes.Email, user.Email),                  // Email
                new Claim(ClaimTypes.Name, user.Name),                    // Name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
            };

            // Step 2: Get the secret key from configuration
            // This key is used to sign the token - NEVER share this publicly!
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
            );

            // Step 3: Create signing credentials
            // HmacSha256 is the algorithm used to sign the token
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Step 4: Create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "TaskManagerAPI",      // Who created the token
                audience: _configuration["Jwt:Audience"] ?? "TaskManagerClient", // Who can use the token
                claims: claims,                                                   // User information
                expires: DateTime.UtcNow.AddHours(24),                          // Token valid for 24 hours
                signingCredentials: credentials                                  // Digital signature
            );

            // Step 5: Write the token as a string
            // This string is what gets sent to the client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /* 
     * HOW JWT AUTHENTICATION WORKS:
     * 
     * 1. USER LOGIN:
     *    - User sends email + password
     *    - Server validates credentials
     *    - Server generates JWT token with user info
     *    - Server sends token to client
     * 
     * 2. SUBSEQUENT REQUESTS:
     *    - Client includes token in Authorization header: "Bearer eyJhbGciOi..."
     *    - Server validates token signature
     *    - Server extracts user info from token
     *    - Server processes request with user context
     * 
     * 3. TOKEN STRUCTURE:
     *    header.payload.signature
     *    eyJhbGci... . eyJ1c2Vy... . SflKxwRJ...
     *    
     *    - Header: Token type and algorithm
     *    - Payload: User claims (data)
     *    - Signature: Cryptographic signature to verify authenticity
     * 
     * SECURITY NOTES:
     * - Token is signed, not encrypted (anyone can read it, but can't modify it)
     * - Never store sensitive data like passwords in tokens
     * - Always use HTTPS in production
     * - Store tokens securely on client (not in localStorage for sensitive apps)
     */
}