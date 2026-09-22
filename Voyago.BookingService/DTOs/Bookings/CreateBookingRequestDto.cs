using System.ComponentModel.DataAnnotations;

namespace Voyago.BookingService.DTOs.Bookings;

public class CreateBookingRequestDto
{
    [Required]
    public Guid ScheduleId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> ScheduleSeatIds { get; set; } = new();
}