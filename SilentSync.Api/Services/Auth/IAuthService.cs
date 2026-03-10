using SilentSync.Api.Controllers;
namespace SilentSync.Api.Services.Auth;

public interface IAuthService
{
    Task<object> RegisterStartAsync(AuthController.RegisterStartRequest req);
    Task<string> RegisterCompleteAsync(AuthController.RegisterCompleteRequest req);
    Task<string> LoginAsync(AuthController.LoginRequest req);
    Task<object> ForgotPasswordAsync(AuthController.ForgotPasswordRequest req);
    Task<string> ResetPasswordAsync(AuthController.ResetPasswordRequest req);
    Task<object> RequestCodeAsync(AuthController.RequestCodeRequest req);
    Task DeleteUserAsync(Guid userId);
    Task<object> GetMeAsync(Guid userId);
}