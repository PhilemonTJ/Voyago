using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Voyago.BookingService.Clients.BusService;
using Voyago.BookingService.Data;
using Voyago.BookingService.DTOs.Bookings;
using Voyago.BookingService.DTOs.Invoices;
using Voyago.BookingService.Exceptions;
using Voyago.BookingService.Models;
using Voyago.BookingService.Services.Interfaces;
using Voyago.Shared.Contracts.Events;

namespace Voyago.BookingService.Services;

public class BookingService : IBookingService
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

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
            throw new ArgumentException("At least one seat must be selected.");
        }

        var seats = await _busServiceClient.ReserveScheduleSeatsAsync(request.ScheduleId, scheduleSeatIds);

        var reservedSeatIds = seats
            .Select(seat => seat.ScheduleSeatId)
            .ToHashSet();

        if (!scheduleSeatIds.All(reservedSeatIds.Contains))
        {
            throw new BusinessConflictException("The requested seats could not be reserved.");
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
            CreatedAt = now,
            ExpiresAt = now.Add(HoldDuration)
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceNumber = GenerateInvoiceNumber(),
            Amount = booking.TotalAmount,
            IssuedAt = now,
            Status = InvoiceStatus.Issued
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

        var bookingCreatedEvent = new BookingCreatedEvent
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            ScheduleId = booking.ScheduleId,
            BookingReference = booking.BookingReference,
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt,
            ScheduleSeatIds = booking.BookingSeats
                .Select(seat => seat.ScheduleSeatId)
                .ToList()
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = nameof(BookingCreatedEvent),
            Payload = JsonSerializer.Serialize(bookingCreatedEvent),
            CreatedAt = now
        };

        _db.Bookings.Add(booking);
        _db.Invoices.Add(invoice);
        _db.OutboxMessages.Add(outboxMessage);

        await _db.SaveChangesAsync();

        return MapToResponse(booking);
    }

    public async Task<BookingResponseDto?> GetByIdAsync(Guid bookingId)
    {
        var booking = await _db.Bookings
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
                SeatCount = booking.BookingSeats.Count,
                Version = EF.Property<uint>(booking, "xmin")
            })
            .ToListAsync();
    }

    public async Task CancelAsync(Guid bookingId, uint expectedVersion)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var booking = await _db.Bookings
            .FromSqlInterpolated($"""
                SELECT *, xmin
                FROM "Bookings"
                WHERE "Id" = {bookingId}
                FOR UPDATE
                """)
            .Include(booking => booking.BookingSeats)
            .FirstOrDefaultAsync(
                booking => booking.Id == bookingId);

        if (booking is null)
        {
            throw new ResourceNotFoundException("The requested booking could not be found.");
        }

        EnsureCanAccess(booking);
        ConcurrencyToken.ApplyExpectedVersion(_db, booking, expectedVersion);

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new BusinessConflictException("Booking is already cancelled.");
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new BusinessConflictException("This booking cannot be cancelled.");
        }

        var scheduleSeatIds = booking.BookingSeats
            .Select(seat => seat.ScheduleSeatId)
            .Distinct()
            .ToList();

        await _busServiceClient.ReleaseScheduleSeatsAsync(booking.ScheduleId, scheduleSeatIds);

        var now = DateTimeOffset.UtcNow;

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;
        booking.UpdatedAt = now;

        var invoice = await _db.Invoices
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Invoices"
                WHERE "BookingId" = {bookingId}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync();

        if (invoice is not null &&
            invoice.Status == InvoiceStatus.Issued)
        {
            invoice.Status = InvoiceStatus.Voided;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<BookingResponseDto> PayInvoiceAsync(Guid bookingId, uint expectedVersion)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var booking = await _db.Bookings
            .FromSqlInterpolated($"""
                SELECT *, xmin
                FROM "Bookings"
                WHERE "Id" = {bookingId}
                FOR UPDATE
                """)
            .Include(booking => booking.BookingSeats)
            .FirstOrDefaultAsync(
                booking => booking.Id == bookingId);

        if (booking is null)
        {
            throw new ResourceNotFoundException("The requested booking could not be found.");
        }

        EnsureCanAccess(booking);
        ConcurrencyToken.ApplyExpectedVersion(_db, booking, expectedVersion);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new BusinessConflictException("Only a pending booking can be paid.");
        }

        var now = DateTimeOffset.UtcNow;

        if (booking.ExpiresAt is not null && booking.ExpiresAt < now)
        {
            throw new BusinessConflictException("This booking has expired and can no longer be paid.");
        }

        var invoice = await _db.Invoices
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Invoices"
                WHERE "BookingId" = {bookingId}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync();

        if (invoice is null)
        {
            throw new ResourceNotFoundException("The requested invoice could not be found.");
        }

        if (invoice.Status != InvoiceStatus.Issued)
        {
            throw new BusinessConflictException("This invoice is not payable.");
        }

        var scheduleSeatIds = booking.BookingSeats
            .Select(seat => seat.ScheduleSeatId)
            .Distinct()
            .ToList();

        await _busServiceClient.ConfirmScheduleSeatsAsync(booking.ScheduleId, scheduleSeatIds);

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = now;
        invoice.PaymentMethod = "Invoice";

        booking.Status = BookingStatus.Confirmed;
        booking.PaidAt = now;
        booking.UpdatedAt = now;

        var bookingConfirmedEvent = new BookingConfirmedEvent
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            ScheduleId = booking.ScheduleId,
            BookingReference = booking.BookingReference,
            TotalAmount = booking.TotalAmount,
            PaidAt = now,
            ScheduleSeatIds = scheduleSeatIds
        };

        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = nameof(BookingConfirmedEvent),
            Payload = JsonSerializer.Serialize(bookingConfirmedEvent),
            CreatedAt = now
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(booking);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceAsync(Guid bookingId)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Id == bookingId);

        if (booking is null)
        {
            throw new ResourceNotFoundException("The requested booking could not be found.");
        }

        EnsureCanAccess(booking);

        var invoice = await _db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice =>
                invoice.BookingId == bookingId);

        if (invoice is null)
        {
            return null;
        }

        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            BookingId = invoice.BookingId,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            IssuedAt = invoice.IssuedAt,
            Status = invoice.Status.ToString(),
            PaidAt = invoice.PaidAt,
            PaymentMethod = invoice.PaymentMethod
        };
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

    private static string GenerateInvoiceNumber()
    {
        return $"INV-{Guid.NewGuid():N}"
            .Substring(0, 16)
            .ToUpperInvariant();
    }

    private BookingResponseDto MapToResponse(
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
            ExpiresAt = booking.ExpiresAt,
            PaidAt = booking.PaidAt,
            Version = _db.Entry(booking).Property<uint>("xmin").CurrentValue,
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
