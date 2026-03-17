using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SilentSync.Api.Models;
using SilentSync.Api.Services.Auth;
using SilentSync.Tests.Helpers;

namespace SilentSync.Tests.Services;

public class AuthServiceTests
{
    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-key-123456789",
                ["Jwt:Issuer"] = "SilentSync",
                ["Jwt:Audience"] = "SilentSync"
            })
            .Build();
    }

    // TEST
    [Fact]
    public async Task DeleteUserByEmailAsync_Should_Delete_User_When_User_Exists()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        db.Users.Add(new AppUser
        {
            Email = "user@test.com",
            Role = "user"
        });

        await db.SaveChangesAsync();

        var service = new AuthService(db, config, fake);

        // Act
        await service.DeleteUserByEmailAsync("user@test.com");

        // Assert
        var deleted = await db.Users.SingleOrDefaultAsync(u => u.Email == "user@test.com");
        Assert.Null(deleted);
    }
}