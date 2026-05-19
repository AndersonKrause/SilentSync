using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilentSync.Api.Services.Rooms;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Contracts.Rooms;
using SilentSync.Api.Data;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly AppDbContext _db;
    
    public RoomsController(IRoomService roomService, AppDbContext db)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    [Authorize(Roles = "host,admin")]
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized("Invalid token.");

        try
        {
            var result = await _roomService.CreateAsync(userId.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(string code, [FromBody] JoinRoomRequest req)
    {
        try
        {
            var result = await _roomService.JoinAsync(code, req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("{code}/join-auth")]
    public async Task<IActionResult> JoinAuth(string code, [FromBody] JoinRoomAuthRequest req)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized("Invalid token.");

        try
        {
            var result = await _roomService.JoinAuthAsync(code, userId.Value, req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{code}/heartbeat")]
    public async Task<IActionResult> Heartbeat(string code, [FromBody] HeartbeatRequest req)
    {
        try
        {
            await _roomService.HeartbeatAsync(code, req);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [Authorize(Roles = "host,admin")]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized("Invalid token.");

        var isAdmin = User.IsInRole("admin");

        var rooms = await _db.Rooms
            .AsNoTracking()
            .Where(r => isAdmin || r.OwnerId == userId.Value)
            .Select(r => new
            {
                id = r.Id,
                code = r.Code,
                ownerId = r.OwnerId,
                membersCount = r.Members.Count
            })
            .OrderBy(r => r.code)
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code)
    {
        try
        {
            var result = await _roomService.GetAsync(code);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize(Roles = "host,admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized("Invalid token.");

        var isAdmin = User.IsInRole("admin");

        var room = await _db.Rooms
            .SingleOrDefaultAsync(r => r.Id == id);

        if (room is null)
            return NotFound("Room not found.");

        if (!isAdmin && room.OwnerId != userId.Value)
            return Forbid();

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }
}
