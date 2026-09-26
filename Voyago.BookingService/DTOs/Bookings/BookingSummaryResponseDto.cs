namespace Voyago.BookingService.DTOs.Bookings;

public class BookingSummaryResponseDto
{
    public Guid Id { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public Guid ScheduleId { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int SeatCount { get; set; }
}