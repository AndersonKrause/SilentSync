using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Controllers;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using System.Security.Cryptography;

namespace SilentSync.Api.Services.Rooms;

public class RoomService : IRoomService
{
    private readonly AppDbContext _db;

    private const int MaxMembers = 500;
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);

    public RoomService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<object> CreateAsync(Guid userId)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = GenerateRoomCode(6);

            var room = new Room
            {
                Code = code,
                OwnerId = userId
            };

            _db.Rooms.Add(room);

            try
            {
                await _db.SaveChangesAsync();
                return new { room.Id, room.Code, room.OwnerId };
            }
            catch (DbUpdateException)
            {
                _db.Entry(room).State = EntityState.Detached;
            }
        }

        throw new InvalidOperationException("A unique RoomCode could not be generated. Please try again.");
    }

    public async Task<object> JoinAsync(string code, RoomsController.JoinRoomRequest req)
    {
        code = Norm(code);

        if (string.IsNullOrWhiteSpace(req.DisplayName) || req.DisplayName.Length > 80)
            throw new ArgumentException("Invalid DisplayName.");

        if (string.IsNullOrWhiteSpace(req.DeviceId) || req.DeviceId.Length > 200)
            throw new ArgumentException("Invalid DeviceId.");

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        var existing = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == room.Id && m.DeviceId == req.DeviceId);

        if (existing is not null)
        {
            existing.LastSeenAtUtc = DateTime.UtcNow;
            existing.DisplayName = req.DisplayName.Trim();
            await _db.SaveChangesAsync();

            return new
            {
                roomCode = room.Code,
                memberId = existing.Id,
                joinedAtUtc = existing.JoinedAtUtc
            };
        }

        var cutoff = DateTime.UtcNow - ActiveWindow;

        var activeCount = await _db.RoomMembers
            .CountAsync(m => m.RoomId == room.Id && m.LastSeenAtUtc >= cutoff);

        if (activeCount >= MaxMembers)
            throw new InvalidOperationException("Full room (capacity limit of 500)");

        var member = new RoomMember
        {
            RoomId = room.Id,
            DisplayName = req.DisplayName.Trim(),
            DeviceId = req.DeviceId.Trim(),
            JoinedAtUtc = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow
        };

        _db.RoomMembers.Add(member);
        await _db.SaveChangesAsync();

        return new
        {
            roomCode = room.Code,
            memberId = member.Id,
            joinedAtUtc = member.JoinedAtUtc
        };
    }

    public async Task<object> JoinAuthAsync(string code, Guid userId, RoomsController.JoinRoomAuthRequest req)
    {
        code = Norm(code);

        if (string.IsNullOrWhiteSpace(req.DisplayName) || req.DisplayName.Length > 80)
            throw new ArgumentException("Invalid DisplayName.");

        if (string.IsNullOrWhiteSpace(req.DeviceId) || req.DeviceId.Length > 200)
            throw new ArgumentException("Invalid DeviceId.");

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        var member = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == room.Id && m.UserId == userId);

        if (member is not null)
        {
            member.LastSeenAtUtc = DateTime.UtcNow;
            member.DisplayName = req.DisplayName.Trim();
            member.DeviceId = req.DeviceId.Trim();
            await _db.SaveChangesAsync();

            return new
            {
                roomCode = room.Code,
                memberId = member.Id,
                joinedAtUtc = member.JoinedAtUtc
            };
        }

        var existingByDevice = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == room.Id && m.DeviceId == req.DeviceId);

        if (existingByDevice is not null)
        {
            existingByDevice.LastSeenAtUtc = DateTime.UtcNow;
            existingByDevice.DisplayName = req.DisplayName.Trim();
            existingByDevice.UserId = userId;
            await _db.SaveChangesAsync();

            return new
            {
                roomCode = room.Code,
                memberId = existingByDevice.Id,
                joinedAtUtc = existingByDevice.JoinedAtUtc
            };
        }

        var cutoff = DateTime.UtcNow - ActiveWindow;

        var activeCount = await _db.RoomMembers
            .CountAsync(m => m.RoomId == room.Id && m.LastSeenAtUtc >= cutoff);

        if (activeCount >= MaxMembers)
            throw new InvalidOperationException("Full room (capacity limit of 500)");

        member = new RoomMember
        {
            RoomId = room.Id,
            UserId = userId,
            DisplayName = req.DisplayName.Trim(),
            DeviceId = req.DeviceId.Trim(),
            JoinedAtUtc = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow
        };

        _db.RoomMembers.Add(member);
        await _db.SaveChangesAsync();

        return new
        {
            roomCode = room.Code,
            memberId = member.Id,
            joinedAtUtc = member.JoinedAtUtc
        };
    }

    public async Task HeartbeatAsync(string code, RoomsController.HeartbeatRequest req)
    {
        code = Norm(code);

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        var member = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.Id == req.MemberId && m.RoomId == room.Id);

        if (member is null)
            throw new KeyNotFoundException("Member not found.");

        member.LastSeenAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<object> GetAsync(string code)
    {
        code = Norm(code);

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        return new { room.Id, room.Code };
    }

    public async Task DeleteAsync(Guid roomId, Guid userId)
    {
        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);
        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        if (room.OwnerId != userId)
            throw new UnauthorizedAccessException("You are not allowed to delete this room.");

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();
    }

    private static string Norm(string code) =>
        (code ?? string.Empty).Trim().ToUpperInvariant();

    private static string GenerateRoomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];

        for (var i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];

        return new string(result);
    }
}