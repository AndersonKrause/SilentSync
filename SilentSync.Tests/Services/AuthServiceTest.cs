using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SilentSync.Api.Models;
using SilentSync.Api.Services.Auth;
using SilentSync.Tests.Helpers;
using SilentSync.Api.Contracts.Auth;

namespace SilentSync.Tests.Services;

public class AuthServiceTests
{
    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-key-for-jwt-1234567890",
                ["Jwt:Issuer"] = "SilentSync",
                ["Jwt:Audience"] = "SilentSync"
            })
            .Build();
    }
    
    private static AppUser CreateUserWithPassword(string email, string password, string role = "user")
    {
        var user = new AppUser
        {
            Email = email,
            Role = role
        };

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);

        return user;
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
    
    [Fact]
    public async Task DeleteUserByEmailAsync_Should_Throw_When_User_Does_Not_Exist()
    {
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        var service = new AuthService(db, config, fake);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteUserByEmailAsync("missing@test.com"));
    }
    
    [Fact]
    public async Task DeleteUserByEmailAsync_Should_Throw_When_User_Is_Admin()
    {
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        db.Users.Add(new AppUser
        {
            Email = "admin@test.com",
            Role = "admin"
        });

        await db.SaveChangesAsync();

        var service = new AuthService(db, config, fake);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteUserByEmailAsync("admin@test.com"));
    }
    
    [Fact]
    public async Task LoginAsync_Should_Throw_When_User_Does_Not_Exist()
    {
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        var service = new AuthService(db, config, fake);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.LoginAsync(new LoginRequest("missing@test.com", "123456")));
    }
    
    [Fact]
    public async Task LoginAsync_Should_Throw_When_Password_Is_Wrong()
    {
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        var user = CreateUserWithPassword("user@test.com", "correct-password");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, config, fake);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.LoginAsync(new LoginRequest("user@test.com", "wrong-password")));
    }
    
    [Fact]
    public async Task LoginAsync_Should_Return_Token_When_Credentials_Are_Correct()
    {
        await using var db = TestDbContextFactory.Create();
        var config = CreateConfiguration();
        var fake = new FakeLoginCodeService();

        var user = CreateUserWithPassword("user@test.com", "correct-password");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, config, fake);

        var token = await service.LoginAsync(
            new LoginRequest("user@test.com", "correct-password"));

        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}