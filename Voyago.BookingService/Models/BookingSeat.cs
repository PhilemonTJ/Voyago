using System.ComponentModel.DataAnnotations;

namespace Voyago.BookingService.Models;

public class BookingSeat
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public Guid ScheduleSeatId { get; set; }

    [Required]
    public Guid SeatId { get; set; }

    [Required, MaxLength(20)]
    public string SeatNumber { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}