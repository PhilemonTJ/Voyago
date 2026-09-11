using Voyago.UserService.DTOs.Users;

namespace Voyago.UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetCurrentUserAsync(Guid userId);
}