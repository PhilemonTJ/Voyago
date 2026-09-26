using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BookingService.DTOs.Bookings;
using Voyago.BookingService.Exceptions;
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
        try
        {
            var booking = await _bookingService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = booking.Id },
                booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid booking request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Booking conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
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
        try
        {
            var booking = await _bookingService.GetByIdAsync(id);

            if (booking is null)
            {
                return NotFound();
            }

            return Ok(booking);
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Passenger")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var cancelled = await _bookingService.CancelAsync(id);

            if (!cancelled)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Booking conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}