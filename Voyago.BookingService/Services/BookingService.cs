using Microsoft.EntityFrameworkCore;
using Voyago.BookingService.Clients.BusService;
using Voyago.BookingService.Data;
using Voyago.BookingService.DTOs.Bookings;
using Voyago.BookingService.Exceptions;
using Voyago.BookingService.Models;
using Voyago.BookingService.Services.Interfaces;

namespace Voyago.BookingService.Services;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _db;
    private readonly IBusServiceClient _busServiceClient;
    private readonly ICurrentUser _currentUser;

    public BookingService(
        BookingDbContext db,
        IBusServiceClient busServiceClient,
        ICurrentUser currentUser)
    {
        _db = db;
        _busServiceClient = busServiceClient;
        _currentUser = currentUser;
    }

    public async Task<BookingResponseDto> CreateAsync(CreateBookingRequestDto request)
    {
        var scheduleSeatIds = request.ScheduleSeatIds.Distinct().ToList();

        if (scheduleSeatIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one seat must be selected.");
        }

        var seats = await _busServiceClient.GetScheduleSeatsAsync(request.ScheduleId, scheduleSeatIds);

        if (seats.Count != scheduleSeatIds.Count)
        {
            throw new InvalidOperationException("One or more selected seats are invalid.");
        }

        var unavailableSeats = seats
            .Where(seat => seat.Status != "Available")
            .ToList();

        if (unavailableSeats.Count > 0)
        {
            throw new InvalidOperationException("One or more selected seats are no longer available.");
        }

        var now = DateTimeOffset.UtcNow;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            ScheduleId = request.ScheduleId,
            BookingReference = GenerateBookingReference(),
            Status = BookingStatus.Pending,
            TotalAmount = seats.Sum(seat => seat.Price),
            CreatedAt = now
        };

        foreach (var seat in seats)
        {
            booking.BookingSeats.Add(new BookingSeat
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ScheduleSeatId = seat.ScheduleSeatId,
                SeatId = seat.SeatId,
                SeatNumber = seat.SeatNumber,
                Price = seat.Price,
                CreatedAt = now
            });
        }

        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponseDto?> GetByIdAsync(Guid bookingId)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(booking => booking.BookingSeats)
            .FirstOrDefaultAsync(
                booking => booking.Id == bookingId);

        if (booking is null)
        {
            return null;
        }

        EnsureCanAccess(booking);

        return MapToResponse(booking);
    }

    public async Task<List<BookingSummaryResponseDto>> GetMyBookingsAsync()
    {
        var userId = _currentUser.UserId;

        return await _db.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.CreatedAt)
            .Select(booking => new BookingSummaryResponseDto
            {
                Id = booking.Id,
                BookingReference = booking.BookingReference,
                ScheduleId = booking.ScheduleId,
                Status = booking.Status.ToString(),
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt,
                SeatCount = booking.BookingSeats.Count
            })
            .ToListAsync();
    }

    public async Task<bool> CancelAsync(Guid bookingId)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(
                booking => booking.Id == bookingId);

        if (booking is null)
        {
            return false;
        }

        EnsureCanAccess(booking);

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("This booking cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTimeOffset.UtcNow;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private void EnsureCanAccess(Booking booking)
    {
        if (_currentUser.IsAdmin)
        {
            return;
        }

        if (booking.UserId == _currentUser.UserId)
        {
            return;
        }

        throw new ForbiddenException("You are not allowed to access this booking.");
    }

    private static string GenerateBookingReference()
    {
        return $"VYG-{Guid.NewGuid():N}"
            .Substring(0, 16)
            .ToUpperInvariant();
    }

    private static BookingResponseDto MapToResponse(
        Booking booking)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            ScheduleId = booking.ScheduleId,
            BookingReference = booking.BookingReference,
            Status = booking.Status.ToString(),
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
            CancelledAt = booking.CancelledAt,
            Seats = booking.BookingSeats
                .Select(seat => new BookingSeatResponseDto
                {
                    Id = seat.Id,
                    ScheduleSeatId = seat.ScheduleSeatId,
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    Price = seat.Price
                })
                .ToList()
        };
    }
}