using Voyago.UserService.DTOs.Auth;
using Voyago.UserService.DTOs.Users;

namespace Voyago.UserService.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse?> LoginAsync(LoginRequest request);
}