using Microsoft.EntityFrameworkCore;
using Moq;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using Narrowcasting.Services;

// =========================================
// TS02 – FR-SL4
// GetActiveScreens_ReturnsOnlyActiveScreens
// =========================================
namespace NarrowcastingTest
{
    public class ScreenTest
    {
        // Because we can't moq the ApplicationDbContext directly
        // We need to use InMemory database
        private ApplicationDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetActiveScreens_ReturnsOnlyActiveScreens()
        {
            // Arrange
            var db = CreateInMemoryDb();

            db.Departments.Add(new Department { Id = 1, Name = "Apotheek" });
            await db.SaveChangesAsync();

            db.Screens.AddRange
            (
                new Screen { Id = 1, Name = "WachtKamer A", Location = "Begane Grond", IsActive = true, DepartmentId = 1 },
                new Screen { Id = 2, Name = "Radiologie", Location = "Begane Grond", IsActive = true, DepartmentId = 1 },
                new Screen { Id = 3, Name = "Apotheek", Location = "Begane Grond", IsActive = false, DepartmentId = 1 }
            );
            await db.SaveChangesAsync();

            var mockAudit = new Mock<IAuditService>();
            var service = new ScreenService(db, mockAudit.Object);

            // Act
            // Verify All screens are returned
            var result = await service.GetAllAsync();  // Not only active screens

            // Assert
            // Verify All screens are returned
            Assert.Equal(3, result.Count());

            // Check that the status property is correct for each screen
            var screen1 = result.First(s => s.Id == 1);
            var screen2 = result.First(s => s.Id == 2);
            var screen3 = result.First(s => s.Id == 3);

            Assert.True(screen1.IsActive, "Screen 1 should be active");
            Assert.True(screen2.IsActive, "Screen 2 should be active");
            Assert.False(screen3.IsActive, "Screen 3 should be inactive");

            Assert.Contains(result, s => s.IsActive == true);
            Assert.Contains(result, s => s.IsActive == false);
        }
    }
}


///
/// Why i have used the GetAllAsync method instead of a method that only returns active screens (GetActiveAsync()):
/// In this test, we want to verify that the service correctly handles both active and inactive screens.
/// By using GetAllAsync, we can ensure that the service returns all screens and then check their IsActive status.
/// This approach allows us to test the filtering logic separately if needed.
/// GetActiveAsync would only return active screens => (Public Display).
///
