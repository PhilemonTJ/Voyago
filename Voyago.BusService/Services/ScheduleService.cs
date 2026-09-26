using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Schedules;
using Voyago.BusService.Exceptions;
using Voyago.BusService.Models;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class ScheduleService : IScheduleService
{
    private readonly BusDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ScheduleService(BusDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
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
            throw new InvalidOperationException("Origin and destination stops must be different.");
        }

        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == request.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        if (!bus.IsActive)
        {
            throw new InvalidOperationException("Cannot create a schedule for an inactive bus.");
        }

        var seats = await _db.Seats
            .Where(seat =>
                seat.BusId == request.BusId &&
                seat.IsActive)
            .ToListAsync();

        if (seats.Count == 0)
        {
            throw new InvalidOperationException("Cannot create a schedule for a bus with no active seats.");
        }

        var originStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.OriginStopId);

        if (originStop is null)
        {
            throw new KeyNotFoundException("Origin stop not found.");
        }

        if (!originStop.IsActive)
        {
            throw new InvalidOperationException("The origin stop is inactive.");
        }

        var destinationStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.DestinationStopId);

        if (destinationStop is null)
        {
            throw new KeyNotFoundException("Destination stop not found.");
        }

        if (!destinationStop.IsActive)
        {
            throw new InvalidOperationException("The destination stop is inactive.");
        }

        var overlaps = await _db.Schedules
            .AnyAsync(schedule =>
                schedule.BusId == request.BusId &&
                schedule.Status != ScheduleStatus.Cancelled &&
                departureTime < schedule.ArrivalTime &&
                arrivalTime > schedule.DepartureTime);

        if (overlaps)
        {
            throw new InvalidOperationException("The bus already has an overlapping active schedule.");
        }

        var datetimeNow = DateTimeOffset.UtcNow;

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            BusId = request.BusId,
            OriginStopId = request.OriginStopId,
            DestinationStopId = request.DestinationStopId,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            BaseSeatPrice = request.BaseSeatPrice,
            Status = ScheduleStatus.Scheduled,
            CreatedAt = datetimeNow
        };

        _db.Schedules.Add(schedule);

        foreach (var seat in seats)
        {
            schedule.ScheduleSeats.Add(new ScheduleSeat
            {
                Id = Guid.NewGuid(),
                SeatId = seat.Id,
                Price = request.BaseSeatPrice,
                Status = ScheduleSeatStatus.Available,
                CreatedAt = datetimeNow
            });
        }

        await _db.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task<List<ScheduleResponseDto>> GetAllAsync()
    {
        return await _db.Schedules
            .AsNoTracking()
            .Where(schedule => 
                schedule.Status == ScheduleStatus.Scheduled &&
                schedule.Bus.IsActive &&
                schedule.OriginStop.IsActive &&
                schedule.DestinationStop.IsActive)
            .OrderBy(schedule => schedule.DepartureTime)
            .Select(schedule => new ScheduleResponseDto
            {
                Id = schedule.Id,
                BusId = schedule.BusId,
                OriginStopId = schedule.OriginStopId,
                DestinationStopId = schedule.DestinationStopId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                BaseSeatPrice = schedule.BaseSeatPrice,
                Status = schedule.Status,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ScheduleResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Schedules
            .AsNoTracking()
            .Where(schedule => 
                schedule.Id == id &&
                schedule.Status == ScheduleStatus.Scheduled &&
                schedule.Bus.IsActive &&
                schedule.OriginStop.IsActive &&
                schedule.DestinationStop.IsActive)
            .OrderBy(schedule => schedule.DepartureTime)
            .Select(schedule => new ScheduleResponseDto
            {
                Id = schedule.Id,
                BusId = schedule.BusId,
                OriginStopId = schedule.OriginStopId,
                DestinationStopId = schedule.DestinationStopId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                BaseSeatPrice = schedule.BaseSeatPrice,
                Status = schedule.Status,
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
            throw new InvalidOperationException("Origin and destination stops must be different.");
        }

        var schedule = await _db.Schedules
            .FirstOrDefaultAsync(schedule => schedule.Id == id);

        if (schedule is null)
        {
            return null;
        }

        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == schedule.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        var originStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.OriginStopId);

        if (originStop is null)
        {
            throw new KeyNotFoundException("Origin stop not found.");
        }

        if (!originStop.IsActive)
        {
            throw new InvalidOperationException("The origin stop is inactive.");
        }

        var destinationStop = await _db.Stops
            .FirstOrDefaultAsync(stop =>
                stop.Id == request.DestinationStopId);

        if (destinationStop is null)
        {
            throw new KeyNotFoundException("Destination stop not found.");
        }

        if (!destinationStop.IsActive)
        {
            throw new InvalidOperationException("The destination stop is inactive.");
        }

        var overlaps = await _db.Schedules
            .AnyAsync(other =>
                other.Id != id &&
                other.BusId == schedule.BusId &&
                other.Status != ScheduleStatus.Cancelled &&
                departureTime < other.ArrivalTime &&
                arrivalTime > other.DepartureTime);

        if (overlaps)
        {
            throw new InvalidOperationException("The bus already has an overlapping active schedule.");
        }

        var datetimeNow = DateTimeOffset.UtcNow;

        schedule.OriginStopId = request.OriginStopId;
        schedule.DestinationStopId = request.DestinationStopId;
        schedule.DepartureTime = departureTime;
        schedule.ArrivalTime = arrivalTime;
        schedule.BaseSeatPrice = request.BaseSeatPrice;
        schedule.UpdatedAt = datetimeNow;

        var availableSeats = await _db.ScheduleSeats
            .Where(scheduleSeat =>
                scheduleSeat.ScheduleId == schedule.Id &&
                scheduleSeat.Status == ScheduleSeatStatus.Available)
            .ToListAsync();

        foreach (var scheduleSeat in availableSeats)
        {
            scheduleSeat.Price = request.BaseSeatPrice;
            scheduleSeat.UpdatedAt = datetimeNow;
        }

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

        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == schedule.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        schedule.Status = ScheduleStatus.Cancelled;
        schedule.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<ScheduleSearchResponseDto>> SearchAsync(ScheduleSearchRequestDto request)
    {
        var origin = request.Origin.Trim();
        var destination = request.Destination.Trim();

        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException(
                "Origin is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException(
                "Destination is required.",
                nameof(request));
        }

        if (origin.Equals(destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Origin and destination must be different.");
        }
        var indiaOffset = TimeSpan.FromHours(5.5);

        var startDate = new DateTimeOffset(
            request.Date.ToDateTime(TimeOnly.MinValue),
            indiaOffset).ToUniversalTime();

        var endDate = startDate.AddDays(1);

        return await _db.Schedules
            .AsNoTracking()
            .Where(schedule =>
                schedule.Status == ScheduleStatus.Scheduled &&
                schedule.Bus.IsActive &&
                schedule.OriginStop.IsActive &&
                schedule.DestinationStop.IsActive &&
                schedule.OriginStop.City == origin &&
                schedule.DestinationStop.City == destination &&
                schedule.DepartureTime >= startDate &&
                schedule.DepartureTime < endDate)
            .OrderBy(schedule => schedule.DepartureTime)
            .Select(schedule => new ScheduleSearchResponseDto
            {
                ScheduleId = schedule.Id,

                BusId = schedule.BusId,
                BusNumber = schedule.Bus.BusNumber,
                BusName = schedule.Bus.BusName,
                BusType = schedule.Bus.BusType.ToString(),

                OriginStopId = schedule.OriginStopId,
                OriginStopName = schedule.OriginStop.Name,
                OriginCity = schedule.OriginStop.City,

                DestinationStopId = schedule.DestinationStopId,
                DestinationStopName = schedule.DestinationStop.Name,
                DestinationCity = schedule.DestinationStop.City,

                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime
            })
            .ToListAsync();
    }
    private void EnsureCanManageBus(Bus bus)
    {
        if (_currentUser.IsAdmin)
        {
            return;
        }

        if (bus.OperatorId != _currentUser.UserId)
        {
            throw new ForbiddenException(
                "You are not authorized to manage schedules for this bus.");
        }
    }

    private static void ValidateTimes(DateTimeOffset departureTime,  DateTimeOffset arrivalTime)
    {
        if (arrivalTime <= departureTime)
        {
            throw new InvalidOperationException(
                "Arrival time must be after departure time.");
        }
    }

    private static ScheduleResponseDto MapToResponse(Schedule schedule)
    {
        return new ScheduleResponseDto
        {
            Id = schedule.Id,
            BusId = schedule.BusId,
            OriginStopId = schedule.OriginStopId,
            DestinationStopId = schedule.DestinationStopId,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            BaseSeatPrice = schedule.BaseSeatPrice,
            Status = schedule.Status,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}