using Microsoft.EntityFrameworkCore;
using Voyago.UserService.Data;
using Voyago.UserService.DTOs.OperatorProfiles;
using Voyago.UserService.Models;
using Voyago.UserService.Services.Interfaces;

namespace Voyago.UserService.Services;

public class OperatorProfileService : IOperatorProfileService
{
    private readonly UserDbContext _db;

    public OperatorProfileService(UserDbContext db)
    {
        _db = db;
    }

    public async Task<OperatorProfileResponse?> GetMyProfileAsync(Guid userId)
    {
        var profile = await _db.OperatorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile =>
                profile.UserId == userId);

        if (profile is null)
        {
            return null;
        }

        return new OperatorProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CompanyName = profile.CompanyName,
            Description = profile.Description,
            ContactNumber = profile.ContactNumber,
            Address = profile.Address,
            LogoImageUrl = profile.LogoImageUrl,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    public async Task<OperatorProfileResponse> CreateAsync(Guid userId, CreateOperatorProfileRequestDto request)
    {
        var existingProfile = await _db.OperatorProfiles
            .FirstOrDefaultAsync(profile =>
                profile.UserId == userId);

        if (existingProfile is not null)
        {
            throw new InvalidOperationException("Operator profile already exists.");
        }

        var companyName = request.CompanyName.Trim();
        var contactNumber = request.ContactNumber.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            throw new ArgumentException("Contact number cannot be empty.");
        }

        var profile = new OperatorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = companyName,
            Description = request.Description?.Trim(),
            ContactNumber = contactNumber,
            Address = request.Address?.Trim(),
            LogoImageUrl = request.LogoImageUrl?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.OperatorProfiles.Add(profile);

        await _db.SaveChangesAsync();

        return new OperatorProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CompanyName = profile.CompanyName,
            Description = profile.Description,
            ContactNumber = profile.ContactNumber,
            Address = profile.Address,
            LogoImageUrl = profile.LogoImageUrl,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    public async Task<OperatorProfileResponse?> UpdateAsync(Guid userId, UpdateOperatorProfileRequestDto request)
    {
        var companyName = request.CompanyName.Trim();
        var contactNumber = request.ContactNumber.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            throw new ArgumentException("Contact number cannot be empty.");
        }

        var profile = await _db.OperatorProfiles
            .FirstOrDefaultAsync(profile =>
                profile.UserId == userId);

        if (profile is null)
        {
            return null;
        }

        profile.CompanyName = companyName;
        profile.Description = request.Description?.Trim();
        profile.ContactNumber = contactNumber;
        profile.Address = request.Address?.Trim();
        profile.LogoImageUrl = request.LogoImageUrl?.Trim();

        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return new OperatorProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CompanyName = profile.CompanyName,
            Description = profile.Description,
            ContactNumber = profile.ContactNumber,
            Address = profile.Address,
            LogoImageUrl = profile.LogoImageUrl,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}