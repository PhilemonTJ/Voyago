using System.ComponentModel.DataAnnotations;

namespace Voyago.BookingService.Models;

public class Booking
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid ScheduleId { get; set; }

    [Required, MaxLength(20)]
    public string BookingReference { get; set; } = string.Empty;

    [Required]
    public BookingStatus Status { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public ICollection<BookingSeat> BookingSeats { get; set; }
        = new List<BookingSeat>();
}