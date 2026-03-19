using SilentSync.Api.Models;

namespace SilentSync.Tests.Helpers;

public static class TestDataFactory
{
    public static AppUser CreateUser(Guid? id = null, string email = "user@test.com", string role = "user")
    {
        return new AppUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            Role = role
        };
    }

    public static Room CreateRoom(Guid ownerId, string code = "ABC123", Guid? id = null)
    {
        return new Room
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            OwnerId = ownerId
        };
    }
}