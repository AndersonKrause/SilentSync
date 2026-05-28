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

        // 1. Trava estrita: Apenas 1 envio a cada 1 minuto por e-mail
        var hasRecentSend = await _db.LoginCodes
            .AnyAsync(x => x.Email == emailAddr && x.CreatedAtUtc > now.AddMinutes(-1));

        if (hasRecentSend)
            throw new InvalidOperationException("Please wait 1 minute to request a new code.");

        // 2. Proteção de Cota: Máximo de 10 tentativas de login por e-mail nas últimas 24 horas
        var dailyCount = await _db.LoginCodes
            .CountAsync(x => x.Email == emailAddr && x.CreatedAtUtc > now.AddDays(-1));

        if (dailyCount >= 10)
            throw new InvalidOperationException("Daily attempt limit exceeded. Please try again tomorrow..");

        // 3. Invalida códigos anteriores ativos para este e-mail (Evita múltiplos PINs válidos)
        var activeCodes = await _db.LoginCodes
            .Where(x => x.Email == emailAddr && x.UsedAtUtc == null && x.ExpiresAtUtc > now)
            .ToListAsync();

        foreach (var activeCode in activeCodes)
        {
            activeCode.ExpiresAtUtc = now; // Expira imediatamente o código antigo
        }

        // 4. Geração do novo PIN
        var code = Random.Shared.Next(100000, 999999).ToString();

        _db.LoginCodes.Add(new LoginCode
        {
            Email = emailAddr,
            Code = code,
            CreatedAtUtc = now, // Certifique-se de preencher a data de criação explicitamente
            ExpiresAtUtc = now.AddMinutes(10),
        });

        await _db.SaveChangesAsync();

        // 5. Envio do e-mail (fora da transação do banco)
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