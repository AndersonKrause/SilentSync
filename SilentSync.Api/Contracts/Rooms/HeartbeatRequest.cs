namespace SilentSync.Api.Contracts.Rooms;

public record HeartbeatRequest(
    Guid MemberId
);