using Claims.Api.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Claims.Api.Tests;

public sealed class ServiceBusMessagingTests
{
    private static readonly ClaimSubmitted Message = new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        DateTime.Parse("2026-01-01T00:00:00Z"));

    [Fact]
    public void ServiceBus_message_contains_json_and_stable_message_id()
    {
        var first = ServiceBusMessageFactory.Create(Message);
        var second = ServiceBusMessageFactory.Create(Message);

        Assert.Equal(Message.EventId.ToString(), first.MessageId);
        Assert.Equal(first.MessageId, second.MessageId);
        Assert.Contains(Message.ClaimId.ToString(), first.Body.ToString());
        Assert.Equal("application/json", first.ContentType);
    }

    [Fact]
    public async Task Valid_message_is_completed()
    {
        var settlement = new FakeMessageSettlement();
        var handler = CreateHandler();

        await handler.HandleAsync(
            BinaryData.FromObjectAsJson(Message),
            deliveryCount: 1,
            settlement,
            CancellationToken.None);

        Assert.Equal(1, settlement.Completed);
        Assert.Equal(0, settlement.Abandoned);
        Assert.Equal(0, settlement.DeadLettered);
    }

    [Fact]
    public async Task Invalid_message_is_abandoned_before_delivery_limit()
    {
        var settlement = new FakeMessageSettlement();
        var handler = CreateHandler(maxDeliveryCount: 5);

        await handler.HandleAsync(
            new BinaryData("not-json"),
            deliveryCount: 1,
            settlement,
            CancellationToken.None);

        Assert.Equal(0, settlement.Completed);
        Assert.Equal(1, settlement.Abandoned);
        Assert.Equal(0, settlement.DeadLettered);
    }

    [Fact]
    public async Task Invalid_message_is_dead_lettered_at_delivery_limit()
    {
        var settlement = new FakeMessageSettlement();
        var handler = CreateHandler(maxDeliveryCount: 3);

        await handler.HandleAsync(
            new BinaryData("not-json"),
            deliveryCount: 3,
            settlement,
            CancellationToken.None);

        Assert.Equal(0, settlement.Completed);
        Assert.Equal(0, settlement.Abandoned);
        Assert.Equal(1, settlement.DeadLettered);
        Assert.Equal("MaximumDeliveryCountExceeded", settlement.DeadLetterReason);
    }

    [Fact]
    public async Task Canceled_message_is_not_settled()
    {
        var settlement = new FakeMessageSettlement();
        var handler = CreateHandler();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.HandleAsync(
            BinaryData.FromObjectAsJson(Message),
            deliveryCount: 1,
            settlement,
            cancellation.Token));

        Assert.Equal(0, settlement.Completed);
        Assert.Equal(0, settlement.Abandoned);
        Assert.Equal(0, settlement.DeadLettered);
    }

    [Fact]
    public void Missing_namespace_registers_in_memory_publisher_without_consumer()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddClaimEventPublisher(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<InMemoryEventPublisher>(provider.GetRequiredService<IEventPublisher>());
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ImplementationType == typeof(ClaimSubmittedConsumer));
    }

    private static ClaimSubmittedMessageHandler CreateHandler(int maxDeliveryCount = 5) =>
        new(NullLogger<ClaimSubmittedMessageHandler>.Instance, maxDeliveryCount);

    private sealed class FakeMessageSettlement : IMessageSettlement
    {
        public int Completed { get; private set; }
        public int Abandoned { get; private set; }
        public int DeadLettered { get; private set; }
        public string? DeadLetterReason { get; private set; }

        public Task CompleteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Completed++;
            return Task.CompletedTask;
        }

        public Task AbandonAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Abandoned++;
            return Task.CompletedTask;
        }

        public Task DeadLetterAsync(string reason, string description, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeadLettered++;
            DeadLetterReason = reason;
            return Task.CompletedTask;
        }
    }
}