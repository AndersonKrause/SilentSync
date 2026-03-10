using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilentSync.Api.Services.Rooms;
using System.Security.Claims;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public record JoinRoomRequest(string DisplayName, string DeviceId);
    public record HeartbeatRequest(Guid MemberId);
    public record JoinRoomAuthRequest(string DisplayName, string DeviceId);

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
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

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized("Invalid token.");

        try
        {
            await _roomService.DeleteAsync(id, userId.Value);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }
}
