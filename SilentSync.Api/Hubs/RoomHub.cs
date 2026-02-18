using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Data;

namespace SilentSync.Api.Hubs;

public class RoomHub(AppDbContext db) : Hub
{
    private static string Group(string code) => $"room:{code}";
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);

    public async Task JoinRoom(string roomCode, Guid memberId)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        var room = await db.Rooms.SingleOrDefaultAsync(r => r.Code == roomCode);
        if (room is null) throw new HubException("Room not found.");

        var member = await db.RoomMembers.SingleOrDefaultAsync(m => m.Id == memberId && m.RoomId == room.Id);
        if (member is null) throw new HubException("Invalid Membrer.");

        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));

        await Clients.Group(Group(roomCode))
            .SendAsync("memberJoined", new { memberId = member.Id, displayName = member.DisplayName });

        await BroadcastActiveCount(room.Id, roomCode);
    }

    public async Task Heartbeat(string roomCode, Guid memberId)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        var room = await db.Rooms.SingleOrDefaultAsync(r => r.Code == roomCode);
        if (room is null) throw new HubException("Room not found.");

        var member = await db.RoomMembers.SingleOrDefaultAsync(m => m.Id == memberId && m.RoomId == room.Id);
        if (member is null) throw new HubException("Invalid Membrer.");

        member.LastSeenAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await BroadcastActiveCount(room.Id, roomCode);
    }

    private async Task BroadcastActiveCount(Guid roomId, string roomCode)
    {
        var cutoff = DateTime.UtcNow - ActiveWindow;

        var active = await db.RoomMembers
            .CountAsync(m => m.RoomId == roomId && m.LastSeenAtUtc >= cutoff);

        await Clients.Group(Group(roomCode))
            .SendAsync("activeCount", new { roomCode, active });
    }
}