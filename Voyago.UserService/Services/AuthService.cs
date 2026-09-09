using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyago.UserService.Data;
using Voyago.UserService.DTOs.Auth;
using Voyago.UserService.DTOs.Users;
using Voyago.UserService.Models;

namespace Voyago.UserService.Services;

public class AuthService : IAuthService
{
    private readonly UserDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(UserDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
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

        return new LoginResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
    }
}