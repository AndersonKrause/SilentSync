namespace SilentSync.Api.Contracts.Auth;

public record ChangeRoleRequest(
    string Email,
    string Role
);