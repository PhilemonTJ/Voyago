namespace Voyago.UserService.Services.Interfaces;

public interface IRefreshTokenService
{
    string GenerateToken();

    string HashToken(string token);
}