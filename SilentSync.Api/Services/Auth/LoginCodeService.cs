using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using SilentSync.Api.Services;

namespace SilentSync.Api.Services.Auth;

public class LoginCodeService : ILoginCodeService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;

    public LoginCodeService(AppDbContext db, IEmailSender email)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public async Task SendLoginCodeAsync(string emailAddr)
    {
        var now = DateTime.UtcNow;

        var recent = await _db.LoginCodes
            .CountAsync(x => x.Email == emailAddr && x.CreatedAtUtc > now.AddMinutes(-1));

        if (recent >= 3)
            throw new InvalidOperationException("Too many codes. Try again in a minute.");

        var code = Random.Shared.Next(100000, 999999).ToString();

        _db.LoginCodes.Add(new LoginCode
        {
            Email = emailAddr,
            Code = code,
            ExpiresAtUtc = now.AddMinutes(10),
        });

        await _db.SaveChangesAsync();

        await _email.SendAsync(
            emailAddr,
            "SilentSync verification code",
            $"Your code is: {code} (valid for 10 minutes)");
    }

    public async Task ConsumeValidCodeAsync(string emailAddr, string code)
    {
        var now = DateTime.UtcNow;

        var lc = await _db.LoginCodes
            .Where(x =>
                x.Email == emailAddr &&
                x.Code == code &&
                x.UsedAtUtc == null &&
                x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (lc is null)
            throw new UnauthorizedAccessException("Invalid or expired code.");

        lc.UsedAtUtc = now;
        await _db.SaveChangesAsync();
    }
}