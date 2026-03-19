using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Models;
using SilentSync.Api.Services.Rooms;
using SilentSync.Tests.Helpers;

namespace SilentSync.Tests.Services;

public class RoomServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Room_When_User_Is_Valid()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);
        var userId = Guid.NewGuid();

        var result = await service.CreateAsync(userId);

        Assert.NotNull(result);

        var room = await db.Rooms.SingleOrDefaultAsync();
        Assert.NotNull(room);
        Assert.Equal(userId, room!.OwnerId);
        Assert.False(string.IsNullOrWhiteSpace(room.Code));
        Assert.Equal(6, room.Code.Length);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Room_When_Room_Exists()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Code = "ABC123",
            OwnerId = Guid.NewGuid()
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        var result = await service.GetAsync("ABC123");

        Assert.NotNull(result);

        var idProp = result.GetType().GetProperty("Id");
        var codeProp = result.GetType().GetProperty("Code");

        Assert.NotNull(idProp);
        Assert.NotNull(codeProp);

        Assert.Equal(room.Id, idProp!.GetValue(result));
        Assert.Equal("ABC123", codeProp!.GetValue(result));
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_User_Is_Not_Owner()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var ownerId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Code = "ABC123",
            OwnerId = ownerId
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(room.Id, anotherUserId));
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Room_When_User_Is_Owner()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var ownerId = Guid.NewGuid();

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Code = "ABC123",
            OwnerId = ownerId
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        await service.DeleteAsync(room.Id, ownerId);

        var deletedRoom = await db.Rooms.SingleOrDefaultAsync(r => r.Id == room.Id);
        Assert.Null(deletedRoom);
    }
}