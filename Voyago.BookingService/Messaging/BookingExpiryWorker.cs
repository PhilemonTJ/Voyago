using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Voyago.BookingService.Clients.BusService;
using Voyago.BookingService.Data;
using Voyago.BookingService.Models;
using Voyago.Shared.Contracts.Events;

namespace Voyago.BookingService.Messaging;

public class BookingExpiryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpiryWorker> _logger;

    public BookingExpiryWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingExpiryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking expiry worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirePendingBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while expiring pending bookings.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ExpirePendingBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var busServiceClient = scope.ServiceProvider.GetRequiredService<IBusServiceClient>();

        var now = DateTimeOffset.UtcNow;

        var expiredBookingIds = await db.Bookings
            .Where(booking =>
                booking.Status == BookingStatus.Pending &&
                booking.ExpiresAt != null &&
                booking.ExpiresAt < now)
            .OrderBy(booking => booking.ExpiresAt)
            .Select(booking => booking.Id)
            .Take(50)
            .ToListAsync(stoppingToken);

        foreach (var bookingId in expiredBookingIds)
        {
            await ExpireBookingAsync(db, busServiceClient, bookingId, stoppingToken);
        }
    }

    private async Task ExpireBookingAsync(
        BookingDbContext db,
        IBusServiceClient busServiceClient,
        Guid bookingId,
        CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

        var booking = await db.Bookings
            .FromSqlInterpolated($"""
                SELECT *, xmin
                FROM "Bookings"
                WHERE "Id" = {bookingId}
                FOR UPDATE
                """)
            .Include(booking => booking.BookingSeats)
            .FirstOrDefaultAsync(stoppingToken);

        if (booking is null || booking.Status != BookingStatus.Pending)
        {
            await transaction.RollbackAsync(stoppingToken);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        if (booking.ExpiresAt is null || booking.ExpiresAt >= now)
        {
            await transaction.RollbackAsync(stoppingToken);
            return;
        }

        var scheduleSeatIds = booking.BookingSeats
            .Select(seat => seat.ScheduleSeatId)
            .Distinct()
            .ToList();

        try
        {
            await busServiceClient.ReleaseScheduleSeatsAsync(booking.ScheduleId, scheduleSeatIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to release seats for expired booking {BookingId}; it will be retried.",
                booking.Id);

            await transaction.RollbackAsync(stoppingToken);
            return;
        }

        var invoice = await db.Invoices
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Invoices"
                WHERE "BookingId" = {bookingId}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync(stoppingToken);

        booking.Status = BookingStatus.Expired;
        booking.UpdatedAt = now;

        if (invoice is not null && invoice.Status == InvoiceStatus.Issued)
        {
            invoice.Status = InvoiceStatus.Voided;
        }

        var bookingExpiredEvent = new BookingExpiredEvent
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            ScheduleId = booking.ScheduleId,
            BookingReference = booking.BookingReference,
            ExpiredAt = now,
            ScheduleSeatIds = scheduleSeatIds
        };

        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = nameof(BookingExpiredEvent),
            Payload = JsonSerializer.Serialize(bookingExpiredEvent),
            CreatedAt = now
        });

        await db.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);

        _logger.LogInformation("Expired booking {BookingId}.", booking.Id);
    }
}
