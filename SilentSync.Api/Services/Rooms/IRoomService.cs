using SilentSync.Api.Controllers;

namespace SilentSync.Api.Services.Rooms;

public interface IRoomService
{
    Task<object> CreateAsync(Guid userId);
    Task<object> JoinAsync(string code, RoomsController.JoinRoomRequest req);
    Task<object> JoinAuthAsync(string code, Guid userId, RoomsController.JoinRoomAuthRequest req);
    Task HeartbeatAsync(string code, RoomsController.HeartbeatRequest req);
    Task<object> GetAsync(string code);
    Task DeleteAsync(Guid roomId, Guid userId);
}