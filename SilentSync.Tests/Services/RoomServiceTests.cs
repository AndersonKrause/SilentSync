using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Services.Rooms;
using SilentSync.Tests.Helpers;

namespace SilentSync.Tests.Services;

public class RoomServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Room_When_User_Is_Valid()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using (db)
        await using (connection)
        {
            var service = new RoomService(db);
            var userId = Guid.NewGuid();

            var user = TestDataFactory.CreateUser(
                id: userId,
                email: "user@test.com");

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await service.CreateAsync(userId);

            Assert.NotNull(result);

            var room = await db.Rooms.SingleOrDefaultAsync();
            Assert.NotNull(room);
            Assert.Equal(userId, room!.OwnerId);
            Assert.False(string.IsNullOrWhiteSpace(room.Code));
            Assert.Equal(6, room.Code.Length);
        }
    }

    [Fact]
    public async Task GetAsync_Should_Return_Room_When_Room_Exists()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using (db)
        await using (connection)
        {
            var service = new RoomService(db);

            var owner = TestDataFactory.CreateUser(email: "owner@test.com");
            db.Users.Add(owner);
            await db.SaveChangesAsync();

            var room = TestDataFactory.CreateRoom(owner.Id, code: "ABC123");
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
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_User_Is_Not_Owner()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using (db)
        await using (connection)
        {
            var service = new RoomService(db);

            var owner = TestDataFactory.CreateUser(email: "owner@test.com");
            db.Users.Add(owner);
            await db.SaveChangesAsync();

            var room = TestDataFactory.CreateRoom(owner.Id, code: "ABC123");
            db.Rooms.Add(room);
            await db.SaveChangesAsync();

            var anotherUserId = Guid.NewGuid();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteAsync(room.Id, anotherUserId));
        }
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Room_When_User_Is_Owner()
    {
        var (db, connection) = TestDbContextFactory.Create();
        await using (db)
        await using (connection)
        {
            var service = new RoomService(db);

            var owner = TestDataFactory.CreateUser(email: "owner@test.com");
            db.Users.Add(owner);
            await db.SaveChangesAsync();

            var room = TestDataFactory.CreateRoom(owner.Id, code: "ABC123");
            db.Rooms.Add(room);
            await db.SaveChangesAsync();

            await service.DeleteAsync(room.Id, owner.Id);

            var deletedRoom = await db.Rooms.SingleOrDefaultAsync(r => r.Id == room.Id);
            Assert.Null(deletedRoom);
        }
    }
}