using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Seats;
using Voyago.BusService.Models;
using Voyago.BusService.Rules;
using Voyago.BusService.Services.Interfaces;
using Voyago.BusService.Exceptions;

namespace Voyago.BusService.Services;

public class SeatService : ISeatService
{
    private readonly BusDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SeatService(BusDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SeatResponseDto> CreateAsync(Guid busId, CreateSeatRequestDto request)
    {
        var bus = await _db.Buses.FirstOrDefaultAsync(bus => bus.Id == busId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        if (!bus.IsActive)
        {
            throw new InvalidOperationException("Cannot add a seat to an inactive bus.");
        }

        BusSeatTypeRules.Validate(bus.BusType, request.SeatType);

        if (request.RowNumber < 1 || request.RowNumber > bus.TotalRows)
        {
            throw new InvalidOperationException($"Row number must be between 1 and {bus.TotalRows}.");
        }

        if (request.ColumnNumber < 1 || request.ColumnNumber > bus.TotalColumns)
        {
            throw new InvalidOperationException($"Column number must be between 1 and {bus.TotalColumns}.");
        }

        var activeSeatCount = await _db.Seats
            .CountAsync(seat =>
                seat.BusId == busId &&
                seat.IsActive);

        if (activeSeatCount >= bus.TotalSeats)
        {
            throw new InvalidOperationException("The bus has reached its maximum seat capacity.");
        }

        var seatNumber = request.SeatNumber.Trim();

        var seatExists = await _db.Seats
            .AnyAsync(seat =>
                seat.BusId == busId &&
                seat.SeatNumber == seatNumber);

        if (seatExists)
        {
            throw new InvalidOperationException("A seat with this number already exists on the bus.");
        }

        var level = GetLevel(request.SeatType);

        var coordinateExists = await _db.Seats
            .AnyAsync(seat =>
                seat.BusId == busId &&
                seat.Level == level &&
                seat.RowNumber == request.RowNumber &&
                seat.ColumnNumber == request.ColumnNumber);

        if (coordinateExists)
        {
            throw new InvalidOperationException("A seat already exists at this position on the bus.");
        }

        var seat = new Seat
        {
            Id = Guid.NewGuid(),
            BusId = busId,
            SeatNumber = seatNumber,
            SeatType = request.SeatType,
            Level = level,
            RowNumber = request.RowNumber,
            ColumnNumber = request.ColumnNumber,
            IsActive = true
        };

        _db.Seats.Add(seat);

        await _db.SaveChangesAsync();

        return MapToResponse(seat);
    }

    public async Task<List<SeatResponseDto>> GetByBusIdAsync(Guid busId)
    {
        var bus = await _db.Buses
            .AsNoTracking()
            .FirstOrDefaultAsync(bus => bus.Id == busId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        return await _db.Seats
            .AsNoTracking()
            .Where(seat =>
                seat.BusId == busId &&
                seat.IsActive)
            .OrderBy(seat => seat.RowNumber)
            .ThenBy(seat => seat.ColumnNumber)
            .Select(seat => new SeatResponseDto
            {
                Id = seat.Id,
                BusId = seat.BusId,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                Level = seat.Level,
                RowNumber = seat.RowNumber,
                ColumnNumber = seat.ColumnNumber,
                IsActive = seat.IsActive
            })
            .ToListAsync();
    }

    public async Task<SeatResponseDto?> GetByIdAsync(Guid seatId)
    {
        return await _db.Seats
            .AsNoTracking()
            .Where(seat => seat.Id == seatId && seat.IsActive)
            .Select(seat => new SeatResponseDto
            {
                Id = seat.Id,
                BusId = seat.BusId,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                Level = seat.Level,
                RowNumber = seat.RowNumber,
                ColumnNumber = seat.ColumnNumber,
                IsActive = seat.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SeatResponseDto?> UpdateAsync(Guid seatId, UpdateSeatRequestDto request)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(seat => seat.Id == seatId);

        if (seat is null)
        {
            return null;
        }

        var bus = await _db.Buses.FirstOrDefaultAsync(bus => bus.Id == seat.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        if (!bus.IsActive)
        {
            throw new InvalidOperationException("Cannot update a seat on an inactive bus.");
        }

        if (!seat.IsActive)
        {
            throw new InvalidOperationException("Cannot update an inactive seat.");
        }

        BusSeatTypeRules.Validate(bus.BusType, request.SeatType);

        if (request.RowNumber < 1 || request.RowNumber > bus.TotalRows)
        {
            throw new InvalidOperationException($"Row number must be between 1 and {bus.TotalRows}.");
        }

        if (request.ColumnNumber < 1 ||request.ColumnNumber > bus.TotalColumns)
        {
            throw new InvalidOperationException($"Column number must be between 1 and {bus.TotalColumns}.");
        }

        var seatNumber = request.SeatNumber.Trim();

        var duplicateSeat = await _db.Seats
            .AnyAsync(other =>
                other.Id != seatId &&
                other.BusId == seat.BusId &&
                other.SeatNumber == seatNumber);

        if (duplicateSeat)
        {
            throw new InvalidOperationException("A seat with this number already exists on the bus.");
        }

        var level = GetLevel(request.SeatType);

        var coordinateExists = await _db.Seats
            .AnyAsync(other =>
                other.Id != seatId &&
                other.BusId == seat.BusId &&
                other.Level == level &&
                other.RowNumber == request.RowNumber &&
                other.ColumnNumber == request.ColumnNumber);

        if (coordinateExists)
        {
            throw new InvalidOperationException("A seat already exists at this position on the bus.");
        }

        seat.SeatNumber = seatNumber;
        seat.SeatType = request.SeatType;
        seat.Level = level;
        seat.RowNumber = request.RowNumber;
        seat.ColumnNumber = request.ColumnNumber;

        await _db.SaveChangesAsync();

        return MapToResponse(seat);
    }

    public async Task<bool> DeleteAsync(Guid seatId)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(seat => seat.Id == seatId);

        if (seat is null)
        {
            return false;
        }

        var bus = await _db.Buses.FirstOrDefaultAsync(bus => bus.Id == seat.BusId);

        if (bus is null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        EnsureCanManageBus(bus);

        if (!seat.IsActive)
        {
            return false;
        }

        seat.IsActive = false;

        await _db.SaveChangesAsync();

        return true;
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
                "You are not authorized to manage this bus.");
        }
    }

    private static SeatResponseDto MapToResponse(Seat seat)
    {
        return new SeatResponseDto
        {
            Id = seat.Id,
            BusId = seat.BusId,
            SeatNumber = seat.SeatNumber,
            SeatType = seat.SeatType,
            Level = seat.Level,
            RowNumber = seat.RowNumber,
            ColumnNumber = seat.ColumnNumber,
            IsActive = seat.IsActive
        };
    }

    private static SeatLevel GetLevel(SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Seater => SeatLevel.Lower,
            SeatType.SleeperLower => SeatLevel.Lower,
            SeatType.SleeperUpper => SeatLevel.Upper,
            _ => throw new ArgumentOutOfRangeException(nameof(seatType))
        };
    }
}