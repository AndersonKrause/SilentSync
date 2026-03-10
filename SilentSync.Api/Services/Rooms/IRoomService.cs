using SilentSync.Api.Contracts.Rooms;

namespace SilentSync.Api.Services.Rooms;

public interface IRoomService
{
    Task<object> CreateAsync(Guid userId);
    Task<object> JoinAsync(string code, JoinRoomRequest req);
    Task<object> JoinAuthAsync(string code, Guid userId, JoinRoomAuthRequest req);
    Task HeartbeatAsync(string code, HeartbeatRequest req);
    Task<object> GetAsync(string code);
    Task DeleteAsync(Guid roomId, Guid userId);
}