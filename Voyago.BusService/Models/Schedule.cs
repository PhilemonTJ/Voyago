using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.Models;

public class Schedule
{
    [Key]
    public Guid Id { get; set; }

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

    [Required]
    public decimal BaseSeatPrice { get; set; }

    [Required]
    public ScheduleStatus Status { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Bus Bus { get; set; } = null!;

    public Stop OriginStop { get; set; } = null!;

    public Stop DestinationStop { get; set; } = null!;

    public ICollection<ScheduleSeat> ScheduleSeats { get; set; } = new List<ScheduleSeat>();
}