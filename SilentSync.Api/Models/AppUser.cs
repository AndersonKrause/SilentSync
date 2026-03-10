namespace SilentSync.Api.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    
    public string Role { get; set; } = "user";

    public string? PasswordHash { get; set; }
    public string? PendingPasswordHash { get; set; }
    
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public ICollection<RoomMember> RoomMembers { get; set; } = new List<RoomMember>();
}