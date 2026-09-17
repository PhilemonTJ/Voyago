using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Buses;
using Voyago.BusService.Models;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class BusService : IBusService
{
    private readonly BusDbContext _db;

    public BusService(BusDbContext db)
    {
        _db = db;
    }

    public async Task<BusResponseDto> CreateAsync(CreateBusRequestDto request)
    {
        var registrationExists =
            await _db.Buses.AnyAsync(
                bus => bus.RegistrationNumber == request.RegistrationNumber);

        if (registrationExists)
        {
            throw new InvalidOperationException("A bus with this registration number already exists.");
        }

        var busNumberExists =
            await _db.Buses.AnyAsync(
                bus =>
                    bus.OperatorId == request.OperatorId &&
                    bus.BusNumber == request.BusNumber);

        if (busNumberExists)
        {
            throw new InvalidOperationException("This bus number already exists for the operator.");
        }

        var bus = new Bus
        {
            Id = Guid.NewGuid(),
            OperatorId = request.OperatorId,
            RegistrationNumber = request.RegistrationNumber,
            BusNumber = request.BusNumber,
            BusName = request.BusName,
            BusType = request.BusType,
            TotalSeats = request.TotalSeats,
            TotalRows = request.TotalRows,
            TotalColumns = request.TotalColumns,
            ImageUrl = request.ImageUrl,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Buses.Add(bus);

        await _db.SaveChangesAsync();

        return MapToResponse(bus);
    }

    public async Task<List<BusResponseDto>> GetAllAsync()
    {
        return await _db.Buses
            .AsNoTracking()
            .Select(bus => new BusResponseDto
            {
                Id = bus.Id,
                OperatorId = bus.OperatorId,
                RegistrationNumber = bus.RegistrationNumber,
                BusNumber = bus.BusNumber,
                BusName = bus.BusName,
                BusType = bus.BusType,
                TotalSeats = bus.TotalSeats,
                TotalRows = bus.TotalRows,
                TotalColumns = bus.TotalColumns,
                ImageUrl = bus.ImageUrl,
                IsActive = bus.IsActive,
                CreatedAt = bus.CreatedAt,
                UpdatedAt = bus.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<BusResponseDto?> GetByIdAsync(Guid id)
    {
        return await _db.Buses
            .AsNoTracking()
            .Where(bus => bus.Id == id)
            .Select(bus => new BusResponseDto
            {
                Id = bus.Id,
                OperatorId = bus.OperatorId,
                RegistrationNumber = bus.RegistrationNumber,
                BusNumber = bus.BusNumber,
                BusName = bus.BusName,
                BusType = bus.BusType,
                TotalSeats = bus.TotalSeats,
                TotalRows = bus.TotalRows,
                TotalColumns = bus.TotalColumns,
                ImageUrl = bus.ImageUrl,
                IsActive = bus.IsActive,
                CreatedAt = bus.CreatedAt,
                UpdatedAt = bus.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BusResponseDto?> UpdateAsync(Guid id, UpdateBusRequestDto request)
    {
        var bus = await _db.Buses.FirstOrDefaultAsync(bus => bus.Id == id);

        if (bus is null)
        {
            return null;
        }

        if (request.TotalSeats < bus.TotalSeats)
        {
            var activeSeatCount = await _db.Seats
                .CountAsync(s =>
                    s.BusId == id &&
                    s.IsActive);

            if (request.TotalSeats < activeSeatCount)
            {
                throw new InvalidOperationException($"Cannot reduce total seats below the current active seat count of {activeSeatCount}.");
            }
        }

        if (request.TotalRows < bus.TotalRows || request.TotalColumns < bus.TotalColumns)
        {
            var maxRow = await _db.Seats
                .Where(s => s.BusId == id && s.IsActive)
                .Select(s => (int?)s.RowNumber)
                .MaxAsync() ?? 0;

            var maxColumn = await _db.Seats
                .Where(s => s.BusId == id && s.IsActive)
                .Select(s => (int?)s.ColumnNumber)
                .MaxAsync() ?? 0;

            if (request.TotalRows < maxRow)
            {
                throw new InvalidOperationException($"Cannot reduce total rows below {maxRow} because an active seat exists at row {maxRow}.");
            }

            if (request.TotalColumns < maxColumn)
            {
                throw new InvalidOperationException($"Cannot reduce total columns below {maxColumn} because an active seat exists at column {maxColumn}.");
            }
        }

        var registrationExists =
            await _db.Buses.AnyAsync(
                other =>
                    other.Id != id &&
                    other.RegistrationNumber == request.RegistrationNumber);

        if (registrationExists)
        {
            throw new InvalidOperationException("A bus with this registration number already exists.");
        }

        var busNumberExists =
            await _db.Buses.AnyAsync(
                other =>
                    other.Id != id &&
                    other.OperatorId == bus.OperatorId &&
                    other.BusNumber == request.BusNumber);

        if (busNumberExists)
        {
            throw new InvalidOperationException("This bus number already exists for the operator.");
        }

        bus.RegistrationNumber = request.RegistrationNumber;
        bus.BusNumber = request.BusNumber;
        bus.BusName = request.BusName;
        bus.BusType = request.BusType;
        bus.TotalSeats = request.TotalSeats;
        bus.TotalRows = request.TotalRows;
        bus.TotalColumns = request.TotalColumns;
        bus.ImageUrl = request.ImageUrl;
        bus.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return MapToResponse(bus);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bus = await _db.Buses
            .FirstOrDefaultAsync(bus => bus.Id == id);

        if (bus is null)
        {
            return false;
        }

        bus.IsActive = false;
        bus.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private static BusResponseDto MapToResponse(Bus bus)
    {
        return new BusResponseDto
        {
            Id = bus.Id,
            OperatorId = bus.OperatorId,
            RegistrationNumber = bus.RegistrationNumber,
            BusNumber = bus.BusNumber,
            BusName = bus.BusName,
            BusType = bus.BusType,
            TotalSeats = bus.TotalSeats,
            TotalRows = bus.TotalRows,
            TotalColumns = bus.TotalColumns,
            ImageUrl = bus.ImageUrl,
            IsActive = bus.IsActive,
            CreatedAt = bus.CreatedAt,
            UpdatedAt = bus.UpdatedAt
        };
    }
}