using Voyago.BusService.Models;

namespace Voyago.BusService.DTOs.Schedules;

public class ScheduleResponseDto
{
    public Guid Id { get; set; }

    public Guid BusId { get; set; }

    public Guid OriginStopId { get; set; }

    public Guid DestinationStopId { get; set; }

    public DateTimeOffset DepartureTime { get; set; }

    public DateTimeOffset ArrivalTime { get; set; }

    public decimal BaseSeatPrice { get; set; }

    public ScheduleStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}