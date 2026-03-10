using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Controllers;
using SilentSync.Api.Data;
using SilentSync.Api.Models;

namespace SilentSync.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly ILoginCodeService _loginCodeService;

    private static readonly PasswordHasher<AppUser> Hasher = new();

    public AuthService(
        AppDbContext db,
        IConfiguration cfg,
        ILoginCodeService loginCodeService)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _loginCodeService = loginCodeService ?? throw new ArgumentNullException(nameof(loginCodeService));
    }

    private static string NormEmail(string s) => (s ?? "").Trim().ToLowerInvariant();

    public async Task<object> RegisterStartAsync(AuthController.RegisterStartRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var password = req.Password ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            throw new ArgumentException("Invalid email.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password too short (min 6).");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
        {
            user = new AppUser { Email = emailAddr };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new InvalidOperationException("User already registered. Use login.");

        if (!string.IsNullOrWhiteSpace(user.PendingPasswordHash))
        {
            await _loginCodeService.SendLoginCodeAsync(emailAddr);
            return new { next = "verify-code", pending = true };
        }

        user.PendingPasswordHash = Hasher.HashPassword(user, password);
        await _db.SaveChangesAsync();

        await _loginCodeService.SendLoginCodeAsync(emailAddr);
        return new { next = "verify-code" };
    }

    public async Task<string> RegisterCompleteAsync(AuthController.RegisterCompleteRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var code = (req.Code ?? "").Trim();

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Missing email or code.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        if (string.IsNullOrWhiteSpace(user.PendingPasswordHash))
            throw new ArgumentException("No pending registration. Start registration again.");

        await _loginCodeService.ConsumeValidCodeAsync(emailAddr, code);

        user.PasswordHash = user.PendingPasswordHash;
        user.PendingPasswordHash = null;
        user.EmailVerifiedAtUtc ??= DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return JwtTokenService.CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
    }

    public async Task<string> LoginAsync(AuthController.LoginRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var password = req.Password ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Missing email or password.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
            throw new UnauthorizedAccessException("Wrong email or password.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new UnauthorizedAccessException("User has no password. Use register.");

        var vr = Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (vr == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Wrong email or password.");

        return JwtTokenService.CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
    }

    public async Task<object> ForgotPasswordAsync(AuthController.ForgotPasswordRequest req)
    {
        var emailAddr = NormEmail(req.Email);

        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            throw new ArgumentException("Invalid email.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
            return new { ok = true };

        await _loginCodeService.SendLoginCodeAsync(emailAddr);
        return new { next = "verify-code" };
    }

    public async Task<string> ResetPasswordAsync(AuthController.ResetPasswordRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var code = (req.Code ?? "").Trim();
        var newPassword = req.NewPassword ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Missing email or code.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new ArgumentException("Password too short (min 6).");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        await _loginCodeService.ConsumeValidCodeAsync(emailAddr, code);

        user.PasswordHash = Hasher.HashPassword(user, newPassword);
        user.PendingPasswordHash = null;
        user.EmailVerifiedAtUtc ??= DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return JwtTokenService.CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
    }

    public async Task<object> RequestCodeAsync(AuthController.RequestCodeRequest req)
    {
        var emailAddr = NormEmail(req.Email);

        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            throw new ArgumentException("Invalid email.");

        await _loginCodeService.SendLoginCodeAsync(emailAddr);
        return new { ok = true };
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var deletedUser = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (deletedUser is null)
            throw new KeyNotFoundException("User not found.");

        _db.Users.Remove(deletedUser);
        await _db.SaveChangesAsync();
    }

    public async Task<object> GetMeAsync(Guid userId)
    {
        var user = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.EmailVerifiedAtUtc,
                u.CreatedAtUtc
            })
            .SingleOrDefaultAsync();

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        return user;
    }
}