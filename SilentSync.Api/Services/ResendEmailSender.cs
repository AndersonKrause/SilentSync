using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SilentSync.Api.Services;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public ResendEmailSender(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task SendAsync(string toEmail, string subject, string text)
    {
        var apiKey = _cfg["Resend:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Resend API key missing.");

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");

        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            from = "SilentSync <onboarding@resend.dev>",
            to = new[] { toEmail },
            subject,
            text
        };

        req.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var res = await _http.SendAsync(req);
        var responseText = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Resend failed: {res.StatusCode} {responseText}");
    }
}