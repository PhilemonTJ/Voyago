using Voyago.BusService.DTOs.ScheduleSeats;
using Voyago.Shared.Contracts.Bus;

namespace Voyago.BusService.Services.Interfaces;

public interface IScheduleSeatService
{
    Task<List<ScheduleSeatResponseDto>> GetByScheduleIdAsync(Guid scheduleId);

    Task<List<ScheduleSeatInfo>> GetForBookingAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds);
}