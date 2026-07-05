using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Narrowcasting.Data;
using Narrowcasting.Models;


namespace NarrowcastingTest;

public class LoginMedewerkerTest
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
    public async Task GetLoginMedewerker_ReturnsCorrectMedewerker()
    {
        // =========================================
        // TS01 – FR-UA2
        // GetLoginMedewerker_ReturnsCorrectMedewerker
        // =========================================

        // Arrange
        var db = CreateInMemoryDb();
        db.Departments.Add(new Department { Id = 1, Name = "Apotheek" });
        await db.SaveChangesAsync();
        db.Users.AddRange
        (
            new ApplicationUser { Id = "1", UserName = "medewerker1", FullName = "Medewerker 1", DepartmentId = 1 },
            new ApplicationUser { Id = "2", UserName = "medewerker2", FullName = "Medewerker 2", DepartmentId = 1 }
        );
        await db.SaveChangesAsync();
        // Act
        var medewerker = await db.Users.FirstOrDefaultAsync(u => u.UserName == "medewerker1");
        // Assert
        Assert.NotNull(medewerker);
        Assert.Equal("Medewerker 1", medewerker.FullName);
    }

    [Fact]
    public async Task GetLoginMedewerker_ReturnsCorrectMedewerker_And_PasswordMatches()
    {
        // =========================================
        // TS01 – FR-UA2
        // Inloggen met een geautoriseerd employee-account
        // =========================================

        // Arrange
        var db = CreateInMemoryDb();

        // 1. Create a Department
        db.Departments.Add(new Department { Id = 1, Name = "Apotheek" });
        await db.SaveChangesAsync();

        // 2. Create the Employee User
        var user = new ApplicationUser
        {
            Id = "1",
            UserName = "medewerker@ziekenhuis.nl",      // Email as username
            NormalizedUserName = "MEDEWERKER@ZIEKENHUIS.NL",
            Email = "medewerker@ziekenhuis.nl",
            NormalizedEmail = "MEDEWERKER@ZIEKENHUIS.NL",
            FullName = "Medewerker 1",
            DepartmentId = 1,
            EmailConfirmed = true // Important: so they don't get blocked
        };

        // 3. HASH THE PASSWORD (This is where the password goes!)
        var hasher = new PasswordHasher<ApplicationUser>();
        user.PasswordHash = hasher.HashPassword(user, "M3d3w3rk3r!");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act - Simulate the login attempt
        var fetchedUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == "medewerker@ziekenhuis.nl");

        // Assert
        // Check if the user was fetched correctly
        Assert.NotNull(fetchedUser);

        // Verify the password
        Assert.NotNull(fetchedUser.PasswordHash); // Ensure the password hash is set

        var passwordVerificationResult = hasher.VerifyHashedPassword(fetchedUser, fetchedUser.PasswordHash, "M3d3w3rk3r!");
        Assert.Equal(PasswordVerificationResult.Success, passwordVerificationResult);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal("Medewerker 1", fetchedUser.FullName);
        Assert.Equal("Apotheek", fetchedUser.Department?.Name); // Extra check

        // THIS is the actual login validation!
        Assert.Equal(PasswordVerificationResult.Success, passwordVerificationResult);
    }
}
