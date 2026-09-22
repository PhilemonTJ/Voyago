using System.Net.Http.Json;
using Voyago.Shared.Contracts.Bus;

namespace Voyago.BookingService.Clients.BusService;

public class BusServiceClient : IBusServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BusServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<ScheduleSeatInfo>> GetScheduleSeatsAsync(Guid scheduleId, IEnumerable<Guid> scheduleSeatIds)
    {
        var client = _httpClientFactory.CreateClient("bus-service");

        var response = await client.PostAsJsonAsync(
            $"api/internal/schedules/{scheduleId}/seats/validate",
            scheduleSeatIds);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<ScheduleSeatInfo>>()
            ?? new List<ScheduleSeatInfo>();
    }
}
