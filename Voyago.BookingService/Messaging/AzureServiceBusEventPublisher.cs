using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Voyago.BookingService.Messaging;

public class AzureServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusEventPublisher(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    public async Task PublishAsync<T>(T @event)
    {
        var body = JsonSerializer.Serialize(@event);

        var message = new ServiceBusMessage(body)
        {
            ContentType = "application/json"
        };

        message.ApplicationProperties["EventType"] = typeof(T).Name;

        await using var sender = _serviceBusClient.CreateSender("booking-created");

        await sender.SendMessageAsync(message);
    }
}