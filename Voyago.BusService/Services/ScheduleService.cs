using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Schedules;
using Voyago.BusService.Models;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class ScheduleService : IScheduleService
{
    private readonly BusDbContext _db;

    public ScheduleService(BusDbContext db)
    {
        _db = db;
    }

    public async Task<ScheduleResponseDto> CreateAsync(CreateScheduleRequestDto request)
    {
        var departureTime = request.DepartureTime.ToUniversalTime();
        var arrivalTime = request.ArrivalTime.ToUniversalTime();

        ValidateTimes(
            departureTime,
            arrivalTime);

        if (request.OriginStopId == request.DestinationStopId)
        {
            throw new InvalidOperationException(
                "Origin and destination stops must be different.");
        }

        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == request.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        if (!bus.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot create a schedule for an inactive bus.");
        }

        var originStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.OriginStopId);

        if (originStop is null)
        {
            throw new KeyNotFoundException(
                "Origin stop not found.");
        }

        if (!originStop.IsActive)
        {
            throw new InvalidOperationException(
                "The origin stop is inactive.");
        }

        var destinationStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.DestinationStopId);

        if (destinationStop is null)
        {
            throw new KeyNotFoundException(
                "Destination stop not found.");
        }

        if (!destinationStop.IsActive)
        {
            throw new InvalidOperationException(
                "The destination stop is inactive.");
        }

        var overlaps = await _db.Schedules
            .AnyAsync(schedule =>
                schedule.BusId == request.BusId &&
                schedule.IsActive &&
                departureTime < schedule.ArrivalTime &&
                arrivalTime > schedule.DepartureTime);

        if (overlaps)
        {
            throw new InvalidOperationException(
                "The bus already has an overlapping active schedule.");
        }

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            BusId = request.BusId,
            OriginStopId = request.OriginStopId,
            DestinationStopId = request.DestinationStopId,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Schedules.Add(schedule);

        await _db.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task<List<ScheduleResponseDto>> GetAllAsync()
    {
        return await _db.Schedules
            .AsNoTracking()
            .OrderBy(schedule => schedule.DepartureTime)
            .Select(schedule => new ScheduleResponseDto
            {
                Id = schedule.Id,
                BusId = schedule.BusId,
                OriginStopId = schedule.OriginStopId,
                DestinationStopId = schedule.DestinationStopId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                IsActive = schedule.IsActive,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ScheduleResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Schedules
            .AsNoTracking()
            .Where(schedule => schedule.Id == id)
            .Select(schedule => new ScheduleResponseDto
            {
                Id = schedule.Id,
                BusId = schedule.BusId,
                OriginStopId = schedule.OriginStopId,
                DestinationStopId = schedule.DestinationStopId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                IsActive = schedule.IsActive,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ScheduleResponseDto?> UpdateAsync(Guid id, UpdateScheduleRequestDto request)
    {
        var departureTime = request.DepartureTime.ToUniversalTime();
        var arrivalTime = request.ArrivalTime.ToUniversalTime();

        ValidateTimes(
            departureTime,
            arrivalTime);

        if (request.OriginStopId == request.DestinationStopId)
        {
            throw new InvalidOperationException(
                "Origin and destination stops must be different.");
        }

        var schedule = await _db.Schedules
            .FirstOrDefaultAsync(schedule => schedule.Id == id);

        if (schedule is null)
        {
            return null;
        }

        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == request.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        if (!bus.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign a schedule to an inactive bus.");
        }

        var originStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.OriginStopId);

        if (originStop is null)
        {
            throw new KeyNotFoundException(
                "Origin stop not found.");
        }

        if (!originStop.IsActive)
        {
            throw new InvalidOperationException(
                "The origin stop is inactive.");
        }

        var destinationStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.DestinationStopId);

        if (destinationStop is null)
        {
            throw new KeyNotFoundException(
                "Destination stop not found.");
        }

        if (!destinationStop.IsActive)
        {
            throw new InvalidOperationException(
                "The destination stop is inactive.");
        }

        var overlaps = await _db.Schedules
            .AnyAsync(other =>
                other.Id != id &&
                other.BusId == request.BusId &&
                other.IsActive &&
                departureTime < other.ArrivalTime &&
                arrivalTime > other.DepartureTime);

        if (overlaps)
        {
            throw new InvalidOperationException(
                "The bus already has an overlapping active schedule.");
        }

        schedule.BusId = request.BusId;
        schedule.OriginStopId = request.OriginStopId;
        schedule.DestinationStopId = request.DestinationStopId;
        schedule.DepartureTime = departureTime;
        schedule.ArrivalTime = arrivalTime;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var schedule = await _db.Schedules
            .FirstOrDefaultAsync(schedule => schedule.Id == id);

        if (schedule is null)
        {
            return false;
        }

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private static void ValidateTimes(DateTimeOffset departureTime,  DateTimeOffset arrivalTime)
    {
        if (arrivalTime <= departureTime)
        {
            throw new InvalidOperationException(
                "Arrival time must be after departure time.");
        }
    }

    private static ScheduleResponseDto MapToResponse(
        Schedule schedule)
    {
        return new ScheduleResponseDto
        {
            Id = schedule.Id,
            BusId = schedule.BusId,
            OriginStopId = schedule.OriginStopId,
            DestinationStopId = schedule.DestinationStopId,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            IsActive = schedule.IsActive,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}