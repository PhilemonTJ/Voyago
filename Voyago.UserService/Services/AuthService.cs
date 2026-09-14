using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Voyago.UserService.Configuration;
using Voyago.UserService.Data;
using Voyago.UserService.DTOs.Auth;
using Voyago.UserService.DTOs.Users;
using Voyago.UserService.Models;
using Voyago.UserService.Services.Interfaces;

namespace Voyago.UserService.Services;

public class AuthService : IAuthService
{
    private readonly UserDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        UserDbContext db, 
        IPasswordHasher<User> passwordHasher, 
        IJwtService jwtService,
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenService refreshTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _db.Users.FirstOrDefaultAsync(user => user.Email == email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            Role = UserRole.Passenger,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(user =>
                user.Email == email &&
                user.DeletedAt == null);

        if (user is null)
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var accessToken = _jwtService.GenerateAccessToken(user);

        var refreshToken = _refreshTokenService.GenerateToken();

        var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
        };

        _db.RefreshTokens.Add(refreshTokenEntity);

        await _db.SaveChangesAsync();

        return new LoginResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return null;
        }

        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            return null;
        }

        if (refreshToken.RevokedAt is not null)
        {
            return null;
        }

        if (refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(user =>
                user.Id == refreshToken.UserId &&
                user.DeletedAt == null);

        if (user is null)
        {
            return null;
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        var newRefreshToken = _refreshTokenService.GenerateToken();

        var newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        var accessToken = _jwtService.GenerateAccessToken(user);

        _db.RefreshTokens.Add(newRefreshTokenEntity);

        await _db.SaveChangesAsync();

        return new LoginResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<bool> LogoutAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return false;
        }

        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash);

        if (refreshToken is null)
        {
            return false;
        }

        if (refreshToken.RevokedAt is not null)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}