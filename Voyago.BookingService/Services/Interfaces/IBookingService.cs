using Voyago.BookingService.DTOs.Bookings;
using Voyago.BookingService.DTOs.Invoices;

namespace Voyago.BookingService.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateAsync(CreateBookingRequestDto request);

    Task<BookingResponseDto?> GetByIdAsync(Guid bookingId);

    Task<List<BookingSummaryResponseDto>> GetMyBookingsAsync();

    Task CancelAsync(Guid bookingId);

    Task<InvoiceResponseDto?> GetInvoiceAsync(Guid bookingId);
}