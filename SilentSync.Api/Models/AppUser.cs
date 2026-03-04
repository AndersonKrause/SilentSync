namespace SilentSync.Api.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";

    public string? PasswordHash { get; set; }          // senha definitiva
    public string? PendingPasswordHash { get; set; }   // senha antes de confirmar email

    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}