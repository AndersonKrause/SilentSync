using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilentSync.Api.Services.Auth;
using System.Security.Claims;
using SilentSync.Api.Contracts.Auth;

namespace SilentSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    //public record RegisterStartRequest(string Email, string Password);
    //public record RegisterCompleteRequest(string Email, string Code);
   //public record LoginRequest(string Email, string Password);
    //public record RequestCodeRequest(string Email);
    //public record ForgotPasswordRequest(string Email);
    //public record ResetPasswordRequest(string Email, string Code, string NewPassword);

    [HttpPost("register-start")]
    public async Task<IActionResult> RegisterStart([FromBody] RegisterStartRequest req)
    {
        try
        {
            var result = await _authService.RegisterStartAsync(req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("register-complete")]
    public async Task<IActionResult> RegisterComplete([FromBody] RegisterCompleteRequest req)
    {
        try
        {
            var token = await _authService.RegisterCompleteAsync(req);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var token = await _authService.LoginAsync(req);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, ex.Message);
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        try
        {
            var token = await _authService.ResetPasswordAsync(req);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] RequestCodeRequest req)
    {
        try
        {
            var result = await _authService.RequestCodeAsync(req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("delete/user")]
    public async Task<IActionResult> DeleteUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token.");

        try
        {
            await _authService.DeleteUserAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token.");

        try
        {
            var user = await _authService.GetMeAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}