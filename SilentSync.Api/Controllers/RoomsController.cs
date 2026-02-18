using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using System.Security.Cryptography;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoomsController(AppDbContext db) => _db = db;
    public record JoinRoomRequest(string DisplayName, string DeviceId);
    private const int MaxMembers = 500;
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);
    public record HeartbeatRequest(Guid MemberId);

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = GenerateRoomCode(6);
            var room = new Room { Code = code };

            _db.Rooms.Add(room);

            try
            {
                await _db.SaveChangesAsync();
                return Ok(new { room.Id, room.Code });
            }
            catch (DbUpdateException)
            {
                // colisão no índice unique, tenta outro
            }
        }

        return Problem("A unique RoomCode could not be generated. Please try again.");
    }
    
    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(string code, [FromBody] JoinRoomRequest req)
    {
        code = code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(req.DisplayName) || req.DisplayName.Length > 80)
            return BadRequest("Invalid DisplayName.");

        if (string.IsNullOrWhiteSpace(req.DeviceId) || req.DeviceId.Length > 200)
            return BadRequest("Invalid DeviceId.");

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            return NotFound("Room not found.");

        var existing = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == room.Id && m.DeviceId == req.DeviceId);

        if (existing is not null)
        {
            existing.LastSeenAtUtc = DateTime.UtcNow;
            existing.DisplayName = req.DisplayName.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { roomCode = room.Code, memberId = existing.Id, joinedAtUtc = existing.JoinedAtUtc });
        }
        
        var cutoff = DateTime.UtcNow - ActiveWindow;

        var activeCount = await _db.RoomMembers
            .CountAsync(m => m.RoomId == room.Id && m.LastSeenAtUtc >= cutoff);

        if (activeCount >= MaxMembers)
            return Conflict("Full room (capacity limit of 500)");

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

        return Ok(new { roomCode = room.Code, memberId = member.Id, joinedAtUtc = member.JoinedAtUtc });
    }
    
    [HttpPost("{code}/heartbeat")]
    public async Task<IActionResult> Heartbeat(string code, [FromBody] HeartbeatRequest req)
    {
        code = code.Trim().ToUpperInvariant();

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == code);
        if (room is null)
            return NotFound("Room not found.");

        var member = await _db.RoomMembers
            .SingleOrDefaultAsync(m => m.Id == req.MemberId && m.RoomId == room.Id);

        if (member is null)
            return NotFound("Membrer not found.");

        member.LastSeenAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static string GenerateRoomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];

        for (int i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];

        return new string(result);
    }
}
