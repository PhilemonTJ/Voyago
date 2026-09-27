using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.ScheduleSeats;
using Voyago.BusService.Models;
using Voyago.BusService.Services.Interfaces;
using Voyago.Shared.Contracts.Bus;

namespace Voyago.BusService.Services;

public class ScheduleSeatService : IScheduleSeatService
{
    private readonly BusDbContext _db;

    public ScheduleSeatService(BusDbContext db)
    {
        _db = db;
    }

    public async Task<List<ScheduleSeatResponseDto>> GetByScheduleIdAsync(Guid scheduleId)
    {
        var schedule = await _db.Schedules
            .AsNoTracking()
            .Where(schedule => schedule.Id == scheduleId)
            .Select(schedule => new
            {
                schedule.Id,
                schedule.Status,
                BusIsActive = schedule.Bus.IsActive
            })
            .SingleOrDefaultAsync();

        if (schedule is null)
        {
            throw new KeyNotFoundException("Schedule not found.");
        }

        if (schedule.Status != ScheduleStatus.Scheduled)
        {
            throw new InvalidOperationException($"Seat availability cannot be viewed because the schedule status is '{schedule.Status}'.");
        }

        if (!schedule.BusIsActive)
        {
            throw new InvalidOperationException("Seat availability cannot be viewed because the bus is inactive.");
        }

        return await _db.ScheduleSeats
            .AsNoTracking()
            .Where(scheduleSeat =>
                scheduleSeat.ScheduleId == scheduleId &&
                scheduleSeat.Seat.IsActive)
            .OrderBy(scheduleSeat => scheduleSeat.Seat.Level)
            .ThenBy(scheduleSeat => scheduleSeat.Seat.RowNumber)
            .ThenBy(scheduleSeat => scheduleSeat.Seat.ColumnNumber)
            .Select(scheduleSeat => new ScheduleSeatResponseDto
            {
                ScheduleSeatId = scheduleSeat.Id,
                SeatId = scheduleSeat.SeatId,
                SeatNumber = scheduleSeat.Seat.SeatNumber,
                SeatType = scheduleSeat.Seat.SeatType.ToString(),
                Level = scheduleSeat.Seat.Level.ToString(),
                RowNumber = scheduleSeat.Seat.RowNumber,
                ColumnNumber = scheduleSeat.Seat.ColumnNumber,
                Price = scheduleSeat.Price,
                Status = scheduleSeat.Status.ToString()
            })
            .ToListAsync();
    }

    public async Task<List<ScheduleSeatInfo>> GetForBookingAsync(
        Guid scheduleId,
        IEnumerable<Guid> scheduleSeatIds)
    {
        var schedule = await _db.Schedules
            .AsNoTracking()
            .Where(schedule => schedule.Id == scheduleId)
            .Select(schedule => new
            {
                schedule.Id,
                schedule.Status,
                BusIsActive = schedule.Bus.IsActive
            })
            .SingleOrDefaultAsync();

        if (schedule is null)
        {
            throw new KeyNotFoundException("Schedule not found.");
        }

        if (schedule.Status != ScheduleStatus.Scheduled)
        {
            throw new InvalidOperationException($"Schedule cannot be booked because its status is '{schedule.Status}'.");
        }

        if (!schedule.BusIsActive)
        {
            throw new InvalidOperationException("Schedule cannot be booked because the bus is inactive.");
        }

        var seatIds = scheduleSeatIds
            .Distinct()
            .ToList();

        if (seatIds.Count == 0)
        {
            return new List<ScheduleSeatInfo>();
        }

        return await _db.ScheduleSeats
            .AsNoTracking()
            .Where(scheduleSeat =>
                scheduleSeat.ScheduleId == scheduleId &&
                seatIds.Contains(scheduleSeat.Id) &&
                scheduleSeat.Seat.IsActive)
            .Select(scheduleSeat => new ScheduleSeatInfo
            {
                ScheduleSeatId = scheduleSeat.Id,
                ScheduleId = scheduleSeat.ScheduleId,
                SeatId = scheduleSeat.SeatId,
                SeatNumber = scheduleSeat.Seat.SeatNumber,
                Price = scheduleSeat.Price,
                Status = scheduleSeat.Status.ToString()
            })
            .ToListAsync();
    }

    public async Task<List<ScheduleSeatInfo>> ReserveForBookingAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds)
    {
        var seatIds = scheduleSeatIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (seatIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one schedule seat is required.",
                nameof(scheduleSeatIds));
        }

        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =await _db.Database.BeginTransactionAsync();

            var schedule = await _db.Schedules
                .AsNoTracking()
                .Where(schedule => schedule.Id == scheduleId)
                .Select(schedule => new
                {
                    schedule.Id,
                    schedule.Status,
                    BusIsActive = schedule.Bus.IsActive
                })
                .SingleOrDefaultAsync();

            if (schedule is null)
            {
                throw new KeyNotFoundException(
                    "Schedule not found.");
            }

            if (schedule.Status != ScheduleStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    $"Schedule cannot be booked because its status is '{schedule.Status}'.");
            }

            if (!schedule.BusIsActive)
            {
                throw new InvalidOperationException(
                    "Schedule cannot be booked because the bus is inactive.");
            }

            var scheduleSeats = await _db.ScheduleSeats
                .FromSqlInterpolated($"""
                SELECT *
                FROM "ScheduleSeats"
                WHERE "ScheduleId" = {scheduleId}
                  AND "Id" = ANY({seatIds.ToArray()})
                ORDER BY "Id"
                FOR UPDATE
                """)
                .Include(scheduleSeat => scheduleSeat.Seat)
                .ToListAsync();

            if (scheduleSeats.Count != seatIds.Count)
            {
                throw new KeyNotFoundException(
                    "One or more requested schedule seats were not found.");
            }

            if (scheduleSeats.Any(scheduleSeat =>
                !scheduleSeat.Seat.IsActive))
            {
                throw new InvalidOperationException(
                    "One or more requested seats are inactive.");
            }

            if (scheduleSeats.Any(scheduleSeat =>
                scheduleSeat.Status != ScheduleSeatStatus.Available))
            {
                throw new InvalidOperationException(
                    "One or more requested seats are no longer available.");
            }

            var now = DateTimeOffset.UtcNow;

            foreach (var scheduleSeat in scheduleSeats)
            {
                scheduleSeat.Status = ScheduleSeatStatus.Booked;
                scheduleSeat.UpdatedAt = now;
            }

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return scheduleSeats
                .Select(scheduleSeat => new ScheduleSeatInfo
                {
                    ScheduleSeatId = scheduleSeat.Id,
                    ScheduleId = scheduleSeat.ScheduleId,
                    SeatId = scheduleSeat.SeatId,
                    SeatNumber = scheduleSeat.Seat.SeatNumber,
                    Price = scheduleSeat.Price,
                    Status = scheduleSeat.Status.ToString()
                })
                .ToList();
        });
    }

    public async Task ReleaseAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds)
    {
        var seatIds = scheduleSeatIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (seatIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one seat must be released.");
        }

        var strategy = _db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
            await _db.Database.BeginTransactionAsync();

            var schedule = await _db.Schedules
                .AsNoTracking()
                .Where(schedule => schedule.Id == scheduleId)
                .Select(schedule => new
                {
                    schedule.Id,
                    schedule.Status,
                    BusIsActive = schedule.Bus.IsActive
                })
                .SingleOrDefaultAsync();

            if (schedule is null)
            {
                throw new KeyNotFoundException(
                    "Schedule not found.");
            }

            if (schedule.Status != ScheduleStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    $"Schedule cannot be modified because its status is '{schedule.Status}'.");
            }

            if (!schedule.BusIsActive)
            {
                throw new InvalidOperationException(
                    "Schedule cannot be modified because the bus is inactive.");
            }

            var seats = await _db.ScheduleSeats
                .FromSqlInterpolated($"""
            SELECT *
            FROM "ScheduleSeats"
            WHERE "ScheduleId" = {scheduleId}
              AND "Id" = ANY({seatIds.ToArray()})
            ORDER BY "Id"
            FOR UPDATE
            """)
                .ToListAsync();

            if (seats.Count != seatIds.Count)
            {
                throw new InvalidOperationException(
                    "One or more selected seats are invalid.");
            }

            var nonBookedSeats = seats
                .Where(scheduleSeat =>
                    scheduleSeat.Status != ScheduleSeatStatus.Booked)
                .ToList();

            if (nonBookedSeats.Count > 0)
            {
                throw new InvalidOperationException(
                    "One or more selected seats cannot be released because they are not booked.");
            }

            var now = DateTimeOffset.UtcNow;

            foreach (var scheduleSeat in seats)
            {
                scheduleSeat.Status = ScheduleSeatStatus.Available;
                scheduleSeat.UpdatedAt = now;
            }

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
        });
    }
}