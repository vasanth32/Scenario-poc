using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using OrderApi.Configuration;
using Shared.Contracts.Messages;

namespace OrderApi.Services;

/// <summary>
/// Implementation of IOrderMessagePublisher that sends messages to Azure Service Bus.
/// </summary>
public class OrderMessagePublisher : IOrderMessagePublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;

    public OrderMessagePublisher(ServiceBusClient serviceBusClient, IOptions<ServiceBusOptions> options)
    {
        _serviceBusClient = serviceBusClient;
        _queueName = options.Value.QueueName;
    }

    public async Task PublishAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)
    {
        // Create a sender for the queue
        await using ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);

        // Serialize the event to JSON
        var json = JsonSerializer.Serialize(orderEvent);
        var messageBody = Encoding.UTF8.GetBytes(json);

        // Create the Service Bus message
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            MessageId = orderEvent.OrderId.ToString(),
            Subject = "OrderCreated"
        };

        // Send the message
        await sender.SendMessageAsync(message, cancellationToken);
    }
}

