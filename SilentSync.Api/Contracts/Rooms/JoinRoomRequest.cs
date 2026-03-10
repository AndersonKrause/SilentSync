namespace SilentSync.Api.Contracts.Rooms;

public record JoinRoomRequest(
    string DisplayName,
    string DeviceId
    );