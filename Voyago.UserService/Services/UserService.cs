using Microsoft.EntityFrameworkCore;
using Voyago.UserService.Data;
using Voyago.UserService.DTOs.Users;
using Voyago.UserService.Services.Interfaces;

namespace Voyago.UserService.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _db;

    public UserService(UserDbContext db)
    {
        _db = db;
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user =>
                user.Id == userId &&
                user.DeletedAt == null);

        if (user is null)
        {
            return null;
        }

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
}