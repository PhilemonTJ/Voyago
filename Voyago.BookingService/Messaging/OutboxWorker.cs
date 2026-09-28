using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Voyago.BookingService.Data;
using Voyago.Shared.Contracts.Events;

namespace Voyago.BookingService.Messaging;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingMessagesAsync(stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<BookingDbContext>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<IEventPublisher>();

        var messages = await db.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Take(20)
            .ToListAsync(stoppingToken);

        foreach (var message in messages)
        {
            try
            {
                var bookingCreatedEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(message.Payload);

                if (bookingCreatedEvent is null)
                {
                    _logger.LogError(
                        "Could not deserialize outbox message {MessageId}.",
                        message.Id);

                    continue;
                }

                await publisher.PublishAsync(bookingCreatedEvent);

                message.ProcessedAt = DateTimeOffset.UtcNow;

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Processed outbox message {MessageId} of type {EventType}.",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}.",
                    message.Id);
            }
        }
    }
}