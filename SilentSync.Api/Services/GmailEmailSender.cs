using System.Net;
using System.Net.Mail;

namespace SilentSync.Api.Services;

public class GmailEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public GmailEmailSender(IConfiguration config) => _config = config;

    public async Task SendAsync(string toEmail, string subject, string text)
    {
        // Diese Daten kommen aus appsettings.json
        var gmailSettings = _config.GetSection("Gmail");
        var host = gmailSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(gmailSettings["Port"] ?? "587");
        var user = gmailSettings["Email"];
        var pass = gmailSettings["AppPassword"];

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            throw new InvalidOperationException("Gmail email settings not found.");
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var message = new MailMessage(user, toEmail, subject, text);
        await client.SendMailAsync(message);
    }
}
