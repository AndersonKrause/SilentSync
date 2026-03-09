namespace SilentSync.Api.Models;

public class RoomMember
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string DisplayName { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}