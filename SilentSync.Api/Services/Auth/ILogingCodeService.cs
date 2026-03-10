namespace SilentSync.Api.Services.Auth;

public interface ILoginCodeService
{
    Task SendLoginCodeAsync(string emailAddr);
    Task ConsumeValidCodeAsync(string emailAddr, string code);
}