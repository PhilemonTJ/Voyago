using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.ScheduleSeats;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class ScheduleSeatService : IScheduleSeatService
{
    private readonly BusDbContext _db;

    public ScheduleSeatService(BusDbContext db)
    {
        _db = db;
    }

    public async Task<List<ScheduleSeatResponseDto>> GetByScheduleIdAsync(
        Guid scheduleId)
    {
        var scheduleExists = await _db.Schedules
            .AnyAsync(schedule => schedule.Id == scheduleId);

        if (!scheduleExists)
        {
            throw new KeyNotFoundException("Schedule not found.");
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
}