namespace SilentSync.Api.Contracts.Auth;

public record RegisterStartRequest(
    string Email, 
    string Password
    );