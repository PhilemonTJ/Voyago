using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Stops;
using Voyago.BusService.Models;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class StopService : IStopService
{
    private readonly BusDbContext _db;

    public StopService(BusDbContext db)
    {
        _db = db;
    }

    public async Task<StopResponseDto> CreateAsync(
        CreateStopRequestDto request)
    {
        var name = request.Name.Trim();
        var city = request.City.Trim();

        var exists = await _db.Stops
            .AnyAsync(stop =>
                stop.Name == name &&
                stop.City == city);

        if (exists)
        {
            throw new InvalidOperationException("A stop with this name already exists in this city.");
        }

        var stop = new Stop
        {
            Id = Guid.NewGuid(),
            Name = name,
            City = city,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Stops.Add(stop);

        await _db.SaveChangesAsync();

        return MapToResponse(stop);
    }

    public async Task<List<StopResponseDto>> GetAllAsync()
    {
        return await _db.Stops
            .AsNoTracking()
            .OrderBy(stop => stop.City)
            .ThenBy(stop => stop.Name)
            .Select(stop => new StopResponseDto
            {
                Id = stop.Id,
                Name = stop.Name,
                City = stop.City,
                IsActive = stop.IsActive,
                CreatedAt = stop.CreatedAt,
                UpdatedAt = stop.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StopResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Stops
            .AsNoTracking()
            .Where(stop => stop.Id == id)
            .Select(stop => new StopResponseDto
            {
                Id = stop.Id,
                Name = stop.Name,
                City = stop.City,
                IsActive = stop.IsActive,
                CreatedAt = stop.CreatedAt,
                UpdatedAt = stop.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StopResponseDto?> UpdateAsync(Guid id, UpdateStopRequestDto request)
    {
        var stop = await _db.Stops.FirstOrDefaultAsync(stop => stop.Id == id);

        if (stop is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        var city = request.City.Trim();

        var duplicateExists = await _db.Stops
            .AnyAsync(other =>
                other.Id != id &&
                other.Name == name &&
                other.City == city);

        if (duplicateExists)
        {
            throw new InvalidOperationException("A stop with this name already exists in this city.");
        }

        stop.Name = name;
        stop.City = city;
        stop.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return MapToResponse(stop);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var stop = await _db.Stops.FirstOrDefaultAsync(stop => stop.Id == id);

        if (stop is null)
        {
            return false;
        }

        stop.IsActive = false;
        stop.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private static StopResponseDto MapToResponse(Stop stop)
    {
        return new StopResponseDto
        {
            Id = stop.Id,
            Name = stop.Name,
            City = stop.City,
            IsActive = stop.IsActive,
            CreatedAt = stop.CreatedAt,
            UpdatedAt = stop.UpdatedAt
        };
    }
}