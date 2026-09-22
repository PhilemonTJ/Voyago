using Voyago.Shared.Contracts.Bus;

namespace Voyago.BookingService.Clients.BusService;

public interface IBusServiceClient
{
    Task<List<ScheduleSeatInfo>> GetScheduleSeatsAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds);
}