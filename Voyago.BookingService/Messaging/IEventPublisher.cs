namespace Voyago.BookingService.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event);
}