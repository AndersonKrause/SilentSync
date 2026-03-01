using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Data;
using System.Collections.Concurrent;
using SilentSync.Api.Models;
using Microsoft.Extensions.Configuration;

namespace SilentSync.Api.Hubs;

public class RoomHub(AppDbContext db, IConfiguration config) : Hub
{
    private static string Group(string code) => $"room:{code}";
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);
    
    private static readonly ConcurrentDictionary<string, PlayerState> _stateByRoom = new();

    private static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private static string Norm(string code) => code.Trim().ToUpperInvariant();


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
    
    private void EnsureMaster(string? masterKey)
    {
        var expected = config["Player:MasterKey"];
        if (string.IsNullOrWhiteSpace(expected) || masterKey != expected)
            throw new HubException("MasterKey inválida.");
    }
    [HubMethodName("JoinAsController")]
    public async Task JoinAsController(string roomCode, string? masterKey)
    {
        EnsureMaster(masterKey);

        roomCode = Norm(roomCode);

        // valida se a sala existe no banco
        var exists = await db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists) throw new HubException("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));
    }
    private static string NormalizeAudioUrl(string audioUrl)
    {
        audioUrl = (audioUrl ?? "").Trim();
        if (string.IsNullOrWhiteSpace(audioUrl)) return "";

        // se o controller mandar http://localhost:5031/media/... ou https://localhost:7135/media/...
        // vira só /media/...
        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var u))
        {
            if (u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                u.Host.Equals("127.0.0.1"))
            {
                return u.PathAndQuery; // "/media/.../audio.mp3"
            }
        }

        // se já for relativo, ok
        if (audioUrl.StartsWith("/")) return audioUrl;

        // senão, deixa como veio
        return audioUrl;
    }
    
    public record UpdatePlayerStateRequest(
        string RoomCode,
        bool IsPlaying,
        long PositionMs,
        string AudioUrl,
        string VideoUrl,
        string? MasterKey
    );
    
    [HubMethodName("GetPlayerState")]
    public PlayerState GetPlayerState(string roomCode)
    {
        roomCode = Norm(roomCode);

        if (_stateByRoom.TryGetValue(roomCode, out var state))
            return state;

        // default (se ainda não foi setado pelo telão)
        return new PlayerState(roomCode, false, 0, NowMs(), "", "");
    }

    [HubMethodName("UpdatePlayerState")]
    public async Task<PlayerState> UpdatePlayerState(UpdatePlayerStateRequest req)
    {
        EnsureMaster(req.MasterKey);

        var roomCode = Norm(req.RoomCode);

        // valida se a sala existe
        var exists = await db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists) throw new HubException("Room not found.");

        // servidor “carimba” o tempo do estado
        var audioUrl = NormalizeAudioUrl(req.AudioUrl);
        var videoUrl = NormalizeAudioUrl(req.VideoUrl);

        var state = new PlayerState(
            RoomCode: roomCode,
            IsPlaying: req.IsPlaying,
            PositionMs: Math.Max(0, req.PositionMs),
            ServerTimeMs: NowMs(),
            AudioUrl: audioUrl,
            VideoUrl: videoUrl
        );

        _stateByRoom[roomCode] = state;

        await Clients.Group(Group(roomCode))
            .SendAsync("playerStateChanged", state);

        return state;
    }
    
    public record TimeSyncResponse(long t0, long t1, long t2);

    [HubMethodName("TimeSync")]
    public TimeSyncResponse TimeSync(long t0)
    {
        var t1 = NowMs(); // server receive time
        var t2 = NowMs(); // server send time
        return new TimeSyncResponse(t0, t1, t2);
    }
    
    [HubMethodName("JoinScreen")]
    public async Task JoinScreen(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        var exists = await db.Rooms.AnyAsync(r => r.Code == roomCode);
        if (!exists) throw new HubException("Room not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomCode));
    }
    
}