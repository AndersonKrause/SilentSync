using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SilentSync.Api.Data;
using SilentSync.Api.Models;
using SilentSync.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration cfg, IEmailSender email) : ControllerBase
{
    public record RequestCodeRequest(string Email);
    public record VerifyCodeRequest(string Email, string Code);

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] RequestCodeRequest req)
    {
        var emailAddr = (req.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(emailAddr) || !emailAddr.Contains("@"))
            return BadRequest("Invalid email.");

        // 6 digits
        var code = Random.Shared.Next(100000, 999999).ToString();

        var lc = new LoginCode
        {
            Email = emailAddr,
            Code = code,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
        };

        db.LoginCodes.Add(lc);
        await db.SaveChangesAsync();

        await email.SendAsync(emailAddr, "Your SilentSync login code", $"Your code is: {code} (valid for 10 minutes)");

        return Ok(new { ok = true });
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest req)
    {
        var emailAddr = (req.Email ?? "").Trim().ToLowerInvariant();
        var code = (req.Code ?? "").Trim();

        if (string.IsNullOrWhiteSpace(emailAddr) || string.IsNullOrWhiteSpace(code))
            return BadRequest("Missing email or code.");

        var now = DateTime.UtcNow;

        var lc = await db.LoginCodes
            .Where(x => x.Email == emailAddr && x.Code == code && x.UsedAtUtc == null && x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (lc is null)
            return Unauthorized("Invalid or expired code.");

        lc.UsedAtUtc = now;

        // get or create user
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == emailAddr);
        if (user is null)
        {
            user = new AppUser { Email = emailAddr };
            db.Users.Add(user);
        }

        await db.SaveChangesAsync();

        var token = CreateJwt(user.Id, user.Email, cfg);

        return Ok(new { token });
    }

    private static string CreateJwt(Guid userId, string email, IConfiguration cfg)
    {
        var jwt = cfg.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new Exception("Jwt:Key missing");
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}