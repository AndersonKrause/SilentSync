using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SilentSync.Api.Services.Auth;

public static class JwtTokenService
{
    public static string CreateAccessJwt(
        Guid userId,
        string email,
        string role,
        IConfiguration cfg)
    {
        var jwt = cfg.GetSection("Jwt");

        var key = jwt["Key"] ?? throw new Exception("Jwt:Key missing");
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
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
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}