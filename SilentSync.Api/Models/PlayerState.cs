namespace SilentSync.Api.Models;

public record PlayerState(
    string RoomCode,
    bool IsPlaying,
    long PositionMs,
    long ServerTimeMs,
    string AudioUrl
);