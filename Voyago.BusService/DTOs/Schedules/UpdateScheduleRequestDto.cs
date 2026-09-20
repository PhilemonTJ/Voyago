using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.DTOs.Schedules;

public class UpdateScheduleRequestDto
{
    [Required]
    public Guid BusId { get; set; }

    [Required]
    public Guid OriginStopId { get; set; }

    [Required]
    public Guid DestinationStopId { get; set; }

    [Required]
    public DateTimeOffset DepartureTime { get; set; }

    [Required]
    public DateTimeOffset ArrivalTime { get; set; }
}