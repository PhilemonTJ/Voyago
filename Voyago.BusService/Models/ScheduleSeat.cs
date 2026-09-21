using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.Models;

public class ScheduleSeat
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ScheduleId { get; set; }

    [Required]
    public Guid SeatId { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public ScheduleSeatStatus Status { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Schedule Schedule { get; set; } = null!;

    public Seat Seat { get; set; } = null!;
}