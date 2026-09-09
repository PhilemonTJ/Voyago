using Voyago.UserService.DTOs.Auth;
using Voyago.UserService.DTOs.Users;

namespace Voyago.UserService.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}