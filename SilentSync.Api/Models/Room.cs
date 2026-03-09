namespace SilentSync.Api.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public Guid? OwnerId { get; set; }
    public AppUser? Owner { get; set; }
    
    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
}
