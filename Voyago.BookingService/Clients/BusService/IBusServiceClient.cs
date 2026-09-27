using Voyago.Shared.Contracts.Bus;

namespace Voyago.BookingService.Clients.BusService;

public interface IBusServiceClient
{
    Task<List<ScheduleSeatInfo>> GetScheduleSeatsAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds);

    Task<List<ScheduleSeatInfo>> ReserveScheduleSeatsAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds);

    Task ReleaseScheduleSeatsAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds);
}