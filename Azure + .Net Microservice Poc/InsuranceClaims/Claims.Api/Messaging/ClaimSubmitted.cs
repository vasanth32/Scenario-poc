using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Claims.Api.Messaging;

public sealed record ClaimSubmitted(
    Guid EventId,
    Guid ClaimId,
    Guid PolicyId,
    DateTime OccurredAtUtc);

public interface IEventPublisher
{
    Task PublishAsync(ClaimSubmitted message, CancellationToken cancellationToken);
}

public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly ConcurrentQueue<ClaimSubmitted> messages = new();

    public Task PublishAsync(ClaimSubmitted message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<ClaimSubmitted> Messages => messages.ToArray();
}

public sealed class AzureServiceBusEventPublisher(ServiceBusClient client) : IEventPublisher
{
    private readonly ServiceBusSender sender = client.CreateSender("claim-submitted");

    public Task PublishAsync(ClaimSubmitted message, CancellationToken cancellationToken)
        => sender.SendMessageAsync(ServiceBusMessageFactory.Create(message), cancellationToken);
}

public static class ServiceBusMessageFactory
{
    public static ServiceBusMessage Create(ClaimSubmitted message) => new(JsonSerializer.Serialize(message))
    {
        MessageId = message.EventId.ToString(),
        ContentType = "application/json",
        Subject = nameof(ClaimSubmitted)
    };
}

public interface IMessageSettlement
{
    Task CompleteAsync(CancellationToken cancellationToken);
    Task AbandonAsync(CancellationToken cancellationToken);
    Task DeadLetterAsync(string reason, string description, CancellationToken cancellationToken);
}

public sealed class ClaimSubmittedMessageHandler(
    ILogger<ClaimSubmittedMessageHandler> logger,
    int maxDeliveryCount = 5)
{
    public async Task HandleAsync(
        BinaryData body,
        int deliveryCount,
        IMessageSettlement settlement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var message = body.ToObjectFromJson<ClaimSubmitted>()
                ?? throw new JsonException("Message body could not be deserialized as ClaimSubmitted.");

            logger.LogInformation(
                "Processed ClaimSubmitted event {EventId} for claim {ClaimId}.",
                message.EventId,
                message.ClaimId);

            await settlement.CompleteAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (deliveryCount >= maxDeliveryCount)
            {
                await settlement.DeadLetterAsync(
                    "MaximumDeliveryCountExceeded",
                    exception.Message,
                    cancellationToken);

                logger.LogWarning(
                    exception,
                    "Dead-lettered message after {DeliveryCount} deliveries.",
                    deliveryCount);
                return;
            }

            await settlement.AbandonAsync(cancellationToken);
            logger.LogWarning(
                exception,
                "Abandoned message; delivery count is {DeliveryCount}.",
                deliveryCount);
        }
    }
}

public sealed class ClaimSubmittedConsumer(
    ServiceBusClient client,
    IConfiguration configuration,
    ILogger<ClaimSubmittedConsumer> logger,
    ILoggerFactory loggerFactory) : BackgroundService
{
    private const string QueueName = "claim-submitted";
    private readonly ClaimSubmittedMessageHandler messageHandler =
        new(
            loggerFactory.CreateLogger<ClaimSubmittedMessageHandler>(),
            configuration.GetValue("ServiceBus:MaxDeliveryCount", 5));
    private readonly ServiceBusProcessor processor = client.CreateProcessor(
        QueueName,
        new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            MaxConcurrentCalls = 1
        });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        try
        {
            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
            processor.ProcessMessageAsync -= ProcessMessageAsync;
            processor.ProcessErrorAsync -= ProcessErrorAsync;
            await processor.DisposeAsync();
        }
    }

    private Task ProcessMessageAsync(ProcessMessageEventArgs args) =>
        messageHandler.HandleAsync(
            args.Message.Body,
            args.Message.DeliveryCount,
            new ProcessMessageSettlement(args),
            args.CancellationToken);

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(
            args.Exception,
            "Service Bus error. Source: {ErrorSource}; Entity: {EntityPath}.",
            args.ErrorSource,
            args.EntityPath);
        return Task.CompletedTask;
    }
}

public sealed class ProcessMessageSettlement(ProcessMessageEventArgs args) : IMessageSettlement
{
    public Task CompleteAsync(CancellationToken cancellationToken) =>
        args.CompleteMessageAsync(args.Message, cancellationToken);

    public Task AbandonAsync(CancellationToken cancellationToken) =>
        args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);

    public Task DeadLetterAsync(string reason, string description, CancellationToken cancellationToken) =>
        args.DeadLetterMessageAsync(args.Message, reason, description, cancellationToken);
}

public static class EventPublisherRegistration
{
    public static IServiceCollection AddClaimEventPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var namespaceName = configuration["ServiceBus:FullyQualifiedNamespace"];
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            services.AddSingleton<InMemoryEventPublisher>();
            services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventPublisher>());
            return services;
        }

        services.AddSingleton(_ => new ServiceBusClient(namespaceName, new DefaultAzureCredential()));
        services.AddSingleton<IEventPublisher, AzureServiceBusEventPublisher>();
        services.AddHostedService<ClaimSubmittedConsumer>();
        return services;
    }
}