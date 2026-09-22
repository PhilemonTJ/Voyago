namespace Voyago.BookingService.DTOs.Bookings;

public class BookingSeatResponseDto
{
    public Guid Id { get; set; }

    public Guid ScheduleSeatId { get; set; }

    public Guid SeatId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public decimal Price { get; set; }
}