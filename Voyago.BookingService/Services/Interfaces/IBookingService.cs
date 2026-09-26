using Voyago.BookingService.DTOs.Bookings;

namespace Voyago.BookingService.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateAsync(CreateBookingRequestDto request);

    Task<BookingResponseDto?> GetByIdAsync(Guid bookingId);

    Task<List<BookingSummaryResponseDto>> GetMyBookingsAsync();

    Task<bool> CancelAsync(Guid bookingId);
}