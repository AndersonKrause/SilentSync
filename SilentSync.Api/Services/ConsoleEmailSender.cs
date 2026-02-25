namespace SilentSync.Api.Services;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;
    
    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string text)
    {
        _logger.LogInformation("--- SIMULATED EMAIL SENT ---");
        _logger.LogInformation($"To: {toEmail} | Subject: {subject}");
        _logger.LogInformation($"Content: {text}");
        _logger.LogInformation("----------------------------");
    
        return Task.CompletedTask;
    }
}