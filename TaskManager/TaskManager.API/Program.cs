using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURATION ====================

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==================== SWAGGER CONFIGURATION ====================
// Swagger generates API documentation and a test interface
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1",
        Description = "API for managing projects and tasks"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==================== DATABASE CONFIGURATION ====================
// Configure PostgreSQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// ==================== JWT AUTHENTICATION CONFIGURATION ====================
// Configure JWT token validation
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagerAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagerClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

builder.Services.AddAuthorization();

// ==================== CORS CONFIGURATION ====================
// Allow React app to make requests to API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React dev servers
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ==================== DEPENDENCY INJECTION ====================
// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== DATABASE MIGRATION ====================
// Automatically apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Apply pending migrations
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ==================== MIDDLEWARE PIPELINE ====================
// Configure the HTTP request pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
    });
}

app.UseHttpsRedirection();

// IMPORTANT: Order matters!
app.UseCors("AllowReactApp");      // 1. CORS must come before auth
app.UseAuthentication();            // 2. Check if user is authenticated
app.UseAuthorization();             // 3. Check if user has permission

app.MapControllers();

app.Run();

/*
 * PROGRAM.CS EXPLAINED:
 * 
 * This is the entry point of your .NET application. It:
 * 1. Configures services (dependency injection)
 * 2. Sets up database connection
 * 3. Configures authentication
 * 4. Sets up the HTTP pipeline
 * 
 * KEY CONCEPTS:
 * 
 * 1. DEPENDENCY INJECTION:
 *    - AddScoped: New instance per HTTP request
 *    - AddSingleton: One instance for entire app lifetime
 *    - AddTransient: New instance every time it's requested
 * 
 * 2. MIDDLEWARE PIPELINE:
 *    - Processes requests in order (top to bottom)
 *    - Each middleware can pass to next or short-circuit
 *    - Order is critical! Auth must come before authorization
 * 
 * 3. JWT AUTHENTICATION:
 *    - TokenValidationParameters define how to validate tokens
 *    - ValidateIssuer/Audience: Check token is for this API
 *    - ValidateLifetime: Check token hasn't expired
 *    - ValidateIssuerSigningKey: Verify signature is valid
 * 
 * 4. CORS (Cross-Origin Resource Sharing):
 *    - Allows React app (localhost:3000) to call API (localhost:5000)
 *    - Without this, browser blocks the requests for security
 * 
 * 5. DATABASE MIGRATION:
 *    - context.Database.Migrate() applies pending migrations
 *    - This creates/updates database schema automatically
 * 
 * 6. SWAGGER:
 *    - Auto-generates API documentation
 *    - Provides test interface at /swagger
 *    - Essential for testing and development
 */