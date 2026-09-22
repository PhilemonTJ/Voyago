namespace Voyago.BookingService.DTOs.Bookings;

public class BookingResponseDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ScheduleId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public List<BookingSeatResponseDto> Seats { get; set; } = new();
}