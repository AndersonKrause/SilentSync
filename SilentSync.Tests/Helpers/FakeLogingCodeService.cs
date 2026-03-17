using SilentSync.Api.Services.Auth;

namespace SilentSync.Tests.Helpers;

public class FakeLoginCodeService : ILoginCodeService
{
    public Task SendLoginCodeAsync(string email)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeValidCodeAsync(string email, string code)
    {
        return Task.CompletedTask;
    }
}