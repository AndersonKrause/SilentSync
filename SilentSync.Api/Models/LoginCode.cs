namespace SilentSync.Api.Models;

public class LoginCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";         // ex: "493821"
    public DateTime ExpiresAtUtc { get; set; }      // agora + 10min
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAtUtc { get; set; }        // marca como usado
}