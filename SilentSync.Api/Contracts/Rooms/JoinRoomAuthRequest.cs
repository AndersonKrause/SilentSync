namespace SilentSync.Api.Contracts.Rooms;

public record JoinRoomAuthRequest(
    string DisplayName,
    string DeviceId
);