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

        return Problem("Não foi possível gerar um RoomCode único. Tente novamente.");
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
