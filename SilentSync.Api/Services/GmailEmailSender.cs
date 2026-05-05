using System.Net;
using System.Net.Mail;

namespace SilentSync.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config) => _config = config;

    public async Task SendAsync(string toEmail, string subject, string text)
    {
        // Diese Daten kommen aus deinem appsettings.json
        var host = "://gmail.com";
        var port = 587;
        var user = "deine.email@gmail.com"; // Deine Adresse aus dem PDF
        var pass = "dein-16-stelliger-app-code"; 

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var message = new MailMessage(user, toEmail, subject, text);
        await client.SendMailAsync(message);
    }
}
