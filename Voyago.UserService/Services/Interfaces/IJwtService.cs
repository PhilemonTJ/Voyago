using Voyago.UserService.Models;

namespace Voyago.UserService.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
}