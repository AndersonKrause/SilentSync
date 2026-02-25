namespace SilentSync.Api.Services;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string text);
}