using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SilentSync.Api.Hubs;

public class RoomHub : Hub
{
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);
    private static readonly ConcurrentDictionary<string, PlayerState> _stateByRoom = new();
    private static readonly ConcurrentDictionary<string, ConnectionMembership> _membershipByConnection = new();

    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public RoomHub(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [Authorize]
    public async Task JoinRoom(string roomCode, Guid memberId)
    {
        roomCode = Norm(roomCode);
        var userId = GetCurrentUserId();

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == roomCode);
        if (room is null)
            throw new HubException("Room not found.");

        var member = await _db.RoomMembers.SingleOrDefaultAsync(m =>
            m.Id == memberId &&
            m.RoomId == room.Id &&
            m.UserId == userId);

        if (member is null)
            throw new HubException("Invalid member for current user.");

        member.LastSeenAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _membershipByConnection[Context.ConnectionId] =
            new ConnectionMembership(room.Id, roomCode, member.Id, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));

        await Clients.Group(Group(roomCode))
            .SendAsync("memberJoined", new
            {
                memberId = member.Id,
                displayName = member.DisplayName
            });

        await BroadcastActiveCount(room.Id, roomCode);
    }

    [Authorize]
    public async Task Heartbeat(string roomCode)
    {
        roomCode = Norm(roomCode);
        var userId = GetCurrentUserId();

        if (!_membershipByConnection.TryGetValue(Context.ConnectionId, out var membership))
            throw new HubException("Connection is not joined to a room.");

        if (membership.RoomCode != roomCode)
            throw new HubException("Connection is not joined to this room.");

        if (membership.UserId != userId)
            throw new HubException("Connection user mismatch.");

        var member = await _db.RoomMembers.SingleOrDefaultAsync(m =>
            m.Id == membership.MemberId &&
            m.RoomId == membership.RoomId &&
            m.UserId == userId);

        if (member is null)
            throw new HubException("Member not found for this connection.");

        member.LastSeenAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await BroadcastActiveCount(membership.RoomId, membership.RoomCode);
    }

    [HubMethodName("GetPlayerState")]
    public PlayerState GetPlayerState(string roomCode)
    {
        roomCode = Norm(roomCode);

        if (_stateByRoom.TryGetValue(roomCode, out var state))
            return state;

        return new PlayerState(roomCode, false, 0, NowMs(), "", "");
    }

    [Authorize]
    [HubMethodName("UpdatePlayerState")]
    public async Task<PlayerState> UpdatePlayerState(PlayerState request)
    {
        var roomCode = Norm(request.RoomCode ?? "");

        var exists = await _db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists)
            throw new HubException("Room not found.");

        await EnsureOwnedRoomAsync(roomCode);

        var normalized = request with
        {
            RoomCode = roomCode,
            AudioUrl = NormalizeAudioUrl(request.AudioUrl),
            VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? "" : request.VideoUrl.Trim(),
            ServerTimeMs = NowMs()
        };

        _stateByRoom[roomCode] = normalized;

        await Clients.Group(Group(roomCode))
            .SendAsync("playerStateChanged", normalized);

        return normalized;
    }

    [Authorize]
    [HubMethodName("JoinAsController")]
    public async Task JoinAsController(string roomCode)
    {
        roomCode = Norm(roomCode);

        var exists = await _db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists)
            throw new HubException("Room not found.");

        await EnsureOwnedRoomAsync(roomCode);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));
    }

    [HubMethodName("JoinScreen")]
    public async Task JoinScreen(string roomCode)
    {
        roomCode = Norm(roomCode);

        var exists = await _db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists)
            throw new HubException("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));
    }

    [HubMethodName("TimeSync")]
    public TimeSyncResponse TimeSync(long t0)
    {
        var t1 = NowMs();
        var t2 = NowMs();

        return new TimeSyncResponse(t0, t1, t2);
    }

    private async Task BroadcastActiveCount(Guid roomId, string roomCode)
    {
        var cutoff = DateTime.UtcNow - ActiveWindow;

        var active = await _db.RoomMembers
            .CountAsync(m => m.RoomId == roomId && m.LastSeenAtUtc >= cutoff);

        await Clients.Group(Group(roomCode))
            .SendAsync("activeCount", new { roomCode, active });
    }

    private async Task<Room> EnsureOwnedRoomAsync(string roomCode)
    {
        roomCode = Norm(roomCode);
        var userId = GetCurrentUserId();

        var room = await _db.Rooms.SingleOrDefaultAsync(r => r.Code == roomCode);
        if (room is null)
            throw new HubException("Room not found.");

        if (room.OwnerId != userId)
            throw new HubException("You are not the owner of this room.");

        return room;
    }

    private Guid GetCurrentUserId()
    {
        var userIdStr =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            Context.User?.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            throw new HubException("Invalid token.");

        return userId;
    }

    private static string Group(string code) => $"room:{code}";

    private static string Norm(string code) =>
        (code ?? string.Empty).Trim().ToUpperInvariant();

    private static long NowMs() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private static string NormalizeAudioUrl(string audioUrl)
    {
        audioUrl = (audioUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(audioUrl))
            return string.Empty;

        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("127.0.0.1"))
            {
                return uri.PathAndQuery;
            }
        }

        if (audioUrl.StartsWith("/"))
            return audioUrl;

        return audioUrl;
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_membershipByConnection.TryRemove(Context.ConnectionId, out var membership))
        {
            await BroadcastActiveCount(membership.RoomId, membership.RoomCode);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public record TimeSyncResponse(long t0, long t1, long t2);
    private record ConnectionMembership(Guid RoomId, string RoomCode, Guid MemberId, Guid UserId);
}