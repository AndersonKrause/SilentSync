using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using SilentSync.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly IEmailSender _email;

    public AuthController(AppDbContext db, IConfiguration cfg, IEmailSender email)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _email = email ?? throw new ArgumentNullException(nameof(email));
    }

    private static readonly PasswordHasher<AppUser> Hasher = new();

    public record RegisterStartRequest(string Email, string Password);
    public record RegisterCompleteRequest(string Email, string Code);

    public record LoginRequest(string Email, string Password);
    public record RequestCodeRequest(string Email);

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Code, string NewPassword);

    // ========= helpers =========
    private static string NormEmail(string s) => (s ?? "").Trim().ToLowerInvariant();

    private async Task<string> SendLoginCodeAsync(string emailAddr)
    {
        // (opcional) rate limit simples
        var now = DateTime.UtcNow;
        var recent = await _db.LoginCodes.CountAsync(x => x.Email == emailAddr && x.CreatedAtUtc > now.AddMinutes(-1));
        if (recent >= 3) throw new InvalidOperationException("Too many codes. Try again in a minute.");

        var code = Random.Shared.Next(100000, 999999).ToString();

        _db.LoginCodes.Add(new LoginCode
        {
            Email = emailAddr,
            Code = code,
            ExpiresAtUtc = now.AddMinutes(10),
        });

        await _db.SaveChangesAsync();

        await _email.SendAsync(emailAddr, "SilentSync verification code", $"Your code is: {code} (valid for 10 minutes)");
        return code;
    }

    private async Task ConsumeValidCodeAsync(string emailAddr, string code)
    {
        var now = DateTime.UtcNow;

        var lc = await _db.LoginCodes
            .Where(x => x.Email == emailAddr && x.Code == code && x.UsedAtUtc == null && x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (lc is null) throw new UnauthorizedAccessException("Invalid or expired code.");

        lc.UsedAtUtc = now;
        await _db.SaveChangesAsync();
    }

    private static string CreateAccessJwt(
        Guid userId, 
        string emailAddr, 
        string role, 
        IConfiguration cfg
        )
    {
        var jwt = cfg.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new Exception("Jwt:Key missing");
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, emailAddr),
            new(ClaimTypes.Role, role),
            new("typ", "access"),
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30), // DEV
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ========= A) REGISTRO =========
    // A1: email + senha -> salva PendingPasswordHash -> manda código
    [HttpPost("register-start")]
    public async Task<IActionResult> RegisterStart([FromBody] RegisterStartRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var password = req.Password ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            return BadRequest("Invalid email.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return BadRequest("Password too short (min 6).");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
        {
            user = new AppUser { Email = emailAddr };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // já tem conta (senha definitiva)
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            return Conflict("User already registered. Use login.");

        // já existe cadastro pendente: NÃO sobrescreve a senha pendente
        // só reenviamos o código
        if (!string.IsNullOrWhiteSpace(user.PendingPasswordHash))
        {
            try { await SendLoginCodeAsync(emailAddr); }
            catch (InvalidOperationException ex) { return StatusCode(429, ex.Message); }

            return Ok(new { next = "verify-code", pending = true });
        }

        // cria o pending agora (primeira vez)
        user.PendingPasswordHash = Hasher.HashPassword(user, password);
        await _db.SaveChangesAsync();

        try { await SendLoginCodeAsync(emailAddr); }
        catch (InvalidOperationException ex) { return StatusCode(429, ex.Message); }

        return Ok(new { next = "verify-code" });
    }

    // A2: email + code -> confirma -> PendingPasswordHash vira PasswordHash -> retorna token
    [HttpPost("register-complete")]
    public async Task<IActionResult> RegisterComplete([FromBody] RegisterCompleteRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var code = (req.Code ?? "").Trim();

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(code))
            return BadRequest("Missing email or code.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null) return Unauthorized("User not found.");
        if (string.IsNullOrWhiteSpace(user.PendingPasswordHash))
            return BadRequest("No pending registration. Start registration again.");

        try
        {
            await ConsumeValidCodeAsync(emailAddr, code);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }

        user.PasswordHash = user.PendingPasswordHash;
        user.PendingPasswordHash = null;
        user.EmailVerifiedAtUtc ??= DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var token = CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
        return Ok(new { token });
    }

    // ========= B) LOGIN (B1) =========
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var password = req.Password ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Missing email or password.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null) return Unauthorized("Wrong email or password.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash)) return Unauthorized("User has no password. Use register.");

        var vr = Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (vr == PasswordVerificationResult.Failed)
            return Unauthorized("Wrong email or password.");

        var token = CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
        return Ok(new { token });
    }

    // ========= ESQUECI SENHA =========
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            return BadRequest("Invalid email.");

        // por segurança, não vaza se existe ou não:
        // mas em DEV podemos validar existência
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null) return Ok(new { ok = true });

        try
        {
            await SendLoginCodeAsync(emailAddr);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, ex.Message);
        }

        return Ok(new { next = "verify-code" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        var code = (req.Code ?? "").Trim();
        var newPassword = req.NewPassword ?? "";

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(code))
            return BadRequest("Missing email or code.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return BadRequest("Password too short (min 6).");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null) return Unauthorized("User not found.");

        try
        {
            await ConsumeValidCodeAsync(emailAddr, code);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }

        user.PasswordHash = Hasher.HashPassword(user, newPassword);
        user.PendingPasswordHash = null;
        user.EmailVerifiedAtUtc ??= DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var token = CreateAccessJwt(user.Id, user.Email, user.Role, _cfg);
        return Ok(new { token });
    }

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] RequestCodeRequest req)
    {
        var emailAddr = NormEmail(req.Email);
        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            return BadRequest("Invalid email.");

        try
        {
            await SendLoginCodeAsync(emailAddr);
            return Ok(new { ok = true });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, ex.Message);
        }
    }
    
    [Authorize]
    [HttpDelete("delete/user")]
    public async Task<IActionResult> DeleteUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized("Invalid token.");
        }
        
        var deletedUser = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (deletedUser is null)
        {
            return NotFound("User not found.");
        }

        _db.Users.Remove(deletedUser);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token.");

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
            return NotFound("User not found.");

        return Ok(user);
    }
}