using Moq;
using Xunit;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace TaskManager.Tests.Tutorial
{
    /// <summary>
    /// TUTORIAL: Your First Unit Test
    /// 
    /// This test checks if we can create a project successfully.
    /// Read the comments carefully to understand each part.
    /// </summary>
    public class SimpleProjectTest
    {
        /// <summary>
        /// TEST 1: Create a project with valid data
        /// 
        /// SCENARIO: User provides a title and description
        /// EXPECTED: Project is created successfully
        /// </summary>
        [Fact]  // [Fact] means "this is a test" - xUnit will run it
        public async Task CreateProject_WithValidData_ReturnsProject()
        {
            // ==================== ARRANGE ====================
            // Set up everything we need for the test

            // 1. Create test data
            var userId = Guid.NewGuid();  // Fake user ID
            var projectTitle = "My Test Project";
            var projectDescription = "This is a test";

            var createDto = new CreateProjectDto
            {
                Title = projectTitle,
                Description = projectDescription
            };

            // 2. Create a MOCK repository (fake database)
            var mockRepository = new Mock<IProjectRepository>();

            // 3. Tell the mock what to do when AddAsync is called
            mockRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Project>()))
                // When AddAsync is called with ANY project...
                .ReturnsAsync((Project p) => p);
            // ...just return that same project (simulate database save)

            // 4. Create the service we're testing (inject the mock)
            var projectService = new ProjectService(mockRepository.Object);
            // .Object gives us the actual mock instance

            // ==================== ACT ====================
            // Run the code we're testing

            var result = await projectService.CreateProjectAsync(createDto, userId);

            // ==================== ASSERT ====================
            // Check that everything worked correctly

            // Check 1: Result should not be null
            Assert.NotNull(result);
            // If this fails, it means CreateProjectAsync returned null

            // Check 2: Title should match what we sent
            Assert.Equal(projectTitle, result.Title);
            // If this fails, the title was changed somehow

            // Check 3: Description should match
            Assert.Equal(projectDescription, result.Description);

            // Check 4: ID should be set (not empty)
            Assert.NotEqual(Guid.Empty, result.Id);
            // If this fails, the ID wasn't generated

            // ==================== VERIFY ====================
            // Check that the mock was called correctly

            mockRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Project>()),
                Times.Once  // Should be called exactly once
            );
            // This ensures CreateProjectAsync actually tried to save to the database

            // ✅ If we get here, all checks passed - test successful!
        }

        /// <summary>
        /// TEST 2: Create a project with empty title
        /// 
        /// SCENARIO: User provides empty title
        /// EXPECTED: ValidationException is thrown
        /// </summary>
        [Fact]
        public async Task CreateProject_WithEmptyTitle_ThrowsException()
        {
            // ==================== ARRANGE ====================

            var userId = Guid.NewGuid();
            var createDto = new CreateProjectDto
            {
                Title = "",  // Empty title - this should fail!
                Description = "Some description"
            };

            var mockRepository = new Mock<IProjectRepository>();
            var projectService = new ProjectService(mockRepository.Object);

            // ==================== ACT & ASSERT ====================
            // We expect this to throw an exception

            await Assert.ThrowsAsync<ValidationException>(
                async () => await projectService.CreateProjectAsync(createDto, userId)
            );
            // This line says: "Run CreateProjectAsync and expect it to throw ValidationException"
            // If it doesn't throw, the test fails
            // If it throws a different exception, the test fails

            // ==================== VERIFY ====================
            // Repository should NOT be called because validation failed first

            mockRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Project>()),
                Times.Never  // Should never be called
            );

            // ✅ Test passes if ValidationException was thrown and repo wasn't called
        }

        /// <summary>
        /// TEST 3: Get a project that doesn't exist
        /// 
        /// SCENARIO: User requests a project that doesn't exist
        /// EXPECTED: Returns null
        /// </summary>
        [Fact]
        public async Task GetProject_ThatDoesNotExist_ReturnsNull()
        {
            // ==================== ARRANGE ====================

            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var mockRepository = new Mock<IProjectRepository>();

            // Tell mock: when GetByIdAsync is called, return null (project not found)
            mockRepository
                .Setup(repo => repo.GetByIdAsync(projectId, userId))
                .ReturnsAsync((Project?)null);  // Simulate "not found"

            var projectService = new ProjectService(mockRepository.Object);

            // ==================== ACT ====================

            var result = await projectService.GetProjectByIdAsync(projectId, userId);

            // ==================== ASSERT ====================

            Assert.Null(result);  // Result should be null

            // ✅ Test passes if result is null
        }

        /// <summary>
        /// TEST 4: Calculate progress correctly
        /// 
        /// SCENARIO: Project has 4 tasks, 2 are completed
        /// EXPECTED: Progress is 50%
        /// </summary>
        [Fact]
        public async Task GetProject_WithTasks_CalculatesProgressCorrectly()
        {
            // ==================== ARRANGE ====================

            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            // Create a project with 4 tasks
            var project = new Project
            {
                Id = projectId,
                Title = "Test Project",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask { IsCompleted = true },   // Completed
                    new ProjectTask { IsCompleted = true },   // Completed
                    new ProjectTask { IsCompleted = false },  // Not completed
                    new ProjectTask { IsCompleted = false }   // Not completed
                }
            };

            var mockRepository = new Mock<IProjectRepository>();
            mockRepository
                .Setup(repo => repo.GetByIdAsync(projectId, userId))
                .ReturnsAsync(project);

            var projectService = new ProjectService(mockRepository.Object);

            // ==================== ACT ====================

            var result = await projectService.GetProjectByIdAsync(projectId, userId);

            // ==================== ASSERT ====================

            Assert.NotNull(result);
            Assert.Equal(4, result.TotalTasks);      // Should have 4 tasks
            Assert.Equal(2, result.CompletedTasks);  // 2 completed
            Assert.Equal(50, result.ProgressPercentage);  // 50% progress

            // ✅ Test passes if progress calculation is correct
        }
    }

    /*
     * ═══════════════════════════════════════════════════════════
     * KEY CONCEPTS EXPLAINED
     * ═══════════════════════════════════════════════════════════
     * 
     * 1. [Fact] ATTRIBUTE
     *    - Marks a method as a test
     *    - xUnit automatically finds and runs these
     *    - Each [Fact] is independent
     * 
     * 2. MOCK<T>
     *    - Creates a fake implementation of an interface
     *    - Mock<IProjectRepository> = fake repository
     *    - We control what it returns
     * 
     * 3. .Setup()
     *    - Tells the mock what to do when a method is called
     *    - Setup(repo => repo.AddAsync(...)) = "when AddAsync is called"
     *    - .ReturnsAsync(...) = "return this value"
     * 
     * 4. It.IsAny<T>()
     *    - Matches any value of type T
     *    - It.IsAny<Project>() = "any project"
     *    - Useful when you don't care about exact values
     * 
     * 5. Assert.X()
     *    - Checks if something is true
     *    - Assert.Equal(a, b) = "a should equal b"
     *    - Assert.NotNull(x) = "x should not be null"
     *    - If assertion fails, test fails
     * 
     * 6. .Verify()
     *    - Checks if a mock method was called
     *    - Times.Once = called exactly once
     *    - Times.Never = never called
     * 
     * 7. async/await in Tests
     *    - Tests can be async like regular methods
     *    - Use Task as return type (not async void)
     *    - await the method being tested
     * 
     * ═══════════════════════════════════════════════════════════
     * HOW TO RUN THESE TESTS
     * ═══════════════════════════════════════════════════════════
     * 
     * IN VISUAL STUDIO:
     * 1. Test → Test Explorer (Ctrl+E, T)
     * 2. Click "Run All" (green play button)
     * 3. See results: ✓ Passed, ✗ Failed
     * 
     * IN COMMAND LINE:
     * cd TaskManager.Tests
     * dotnet test
     * 
     * ═══════════════════════════════════════════════════════════
     * READING TEST RESULTS
     * ═══════════════════════════════════════════════════════════
     * 
     * ✓ PASSED (Green)
     * - Test ran and all assertions passed
     * - Code works as expected
     * 
     * ✗ FAILED (Red)
     * - Test ran but an assertion failed
     * - Click on test to see which assertion failed
     * - Fix the code and run again
     * 
     * ! ERROR (Yellow)
     * - Test couldn't run (compilation error, missing dependency)
     * - Fix the error and try again
     * 
     * ═══════════════════════════════════════════════════════════
     */
}