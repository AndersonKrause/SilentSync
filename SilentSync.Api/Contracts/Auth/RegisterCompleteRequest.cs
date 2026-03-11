namespace SilentSync.Api.Contracts.Auth;

public record RegisterCompleteRequest(
    string Email, 
    string Code
    );