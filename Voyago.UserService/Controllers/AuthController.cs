using Microsoft.AspNetCore.Mvc;
using Voyago.UserService.DTOs.Auth;
using Voyago.UserService.DTOs.Users;
using Voyago.UserService.Services.Interfaces;

namespace Voyago.UserService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);

            return CreatedAtAction(
                nameof(Register),
                new { id = user.Id },
                user);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid email or password.");
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh(RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (result is null)
        {
            return Problem(
                 statusCode: StatusCodes.Status401Unauthorized,
                 title: "Unauthorized",
                 detail: "Invalid or expired refresh token.");
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequestDto request)
    {
        var result =
            await _authService.LogoutAsync(request);

        if (!result)
        {
            return Problem(
               statusCode: StatusCodes.Status401Unauthorized,
               title: "Unauthorized",
               detail: "Invalid or already revoked refresh token.");
        }

        return NoContent();
    }
}