using SilentSync.Api.Contracts.Auth;

namespace SilentSync.Api.Services.Auth;

public interface IAuthService
{
    Task<object> RegisterStartAsync(RegisterStartRequest req);
    Task<string> RegisterCompleteAsync(RegisterCompleteRequest req);
    Task<string> LoginAsync(LoginRequest req);
    Task<object> ForgotPasswordAsync(ForgotPasswordRequest req);
    Task<string> ResetPasswordAsync(ResetPasswordRequest req);
    Task<object> RequestCodeAsync(RequestCodeRequest req);
    Task DeleteUserAsync(Guid userId);
    Task<object> GetMeAsync(Guid userId);
    Task DeleteUserByEmailAsync(String email);
}