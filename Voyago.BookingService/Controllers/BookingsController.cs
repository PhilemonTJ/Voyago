using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BookingService.DTOs.Bookings;
using Voyago.BookingService.DTOs.Invoices;
using Voyago.BookingService.Services.Interfaces;

namespace Voyago.BookingService.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(
        IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Passenger")]
    public async Task<ActionResult<BookingResponseDto>> Create(CreateBookingRequestDto request)
    {

        var booking = await _bookingService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = booking.Id },
            booking);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Passenger")]
    public async Task<ActionResult<List<BookingSummaryResponseDto>>> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync();

        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);

        if (booking is null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Passenger")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _bookingService.CancelAsync(id);

        return NoContent();
    }

    [HttpGet("{bookingId:guid}/invoice")]
    [Authorize]
    public async Task<ActionResult<InvoiceResponseDto>> GetInvoice(Guid bookingId)
    {
        var invoice = await _bookingService.GetInvoiceAsync(bookingId);

        if (invoice is null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }
}