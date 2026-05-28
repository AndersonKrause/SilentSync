using System.Net;
using System.Net.Mail;

namespace SilentSync.Api.Services;

public class GmailEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public GmailEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string text)
    {
        var gmailSettings = _config.GetSection("Gmail");

        var host = gmailSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(gmailSettings["Port"] ?? "587");

        var user = gmailSettings["Email"];
        var pass = gmailSettings["AppPassword"];

        if (string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass))
        {
            throw new InvalidOperationException(
                "Gmail configuration missing."
            );
        }

        using var client = new SmtpClient(host, port);

        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;

        client.Credentials = new NetworkCredential(user, pass);

        client.Timeout = 15000;

        using var message = new MailMessage(
            from: user,
            to: toEmail,
            subject,
            body: text
        );

        await client.SendMailAsync(message);
    }
}