namespace Voyago.BusService.DTOs.Schedules;

public class ScheduleSearchResponseDto
{
    public Guid ScheduleId { get; set; }

    public Guid BusId { get; set; }

    public string BusNumber { get; set; } = string.Empty;

    public string? BusName { get; set; }

    public string BusType { get; set; } = string.Empty;

    public Guid OriginStopId { get; set; }

    public string OriginStopName { get; set; } = string.Empty;

    public string OriginCity { get; set; } = string.Empty;

    public Guid DestinationStopId { get; set; }

    public string DestinationStopName { get; set; } = string.Empty;

    public string DestinationCity { get; set; } = string.Empty;

    public DateTimeOffset DepartureTime { get; set; }

    public DateTimeOffset ArrivalTime { get; set; }
}