using Voyago.BusService.DTOs.ScheduleSeats;

namespace Voyago.BusService.Services.Interfaces;

public interface IScheduleSeatService
{
    Task<List<ScheduleSeatResponseDto>> GetByScheduleIdAsync(Guid scheduleId);
}