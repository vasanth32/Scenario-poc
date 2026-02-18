using Azure.Messaging.ServiceBus;
using OrderApi.Configuration;
using OrderApi.Services;
using Shared.Contracts.Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Service Bus options
builder.Services.Configure<ServiceBusOptions>(
    builder.Configuration.GetSection(ServiceBusOptions.SectionName));

var serviceBusOptions = builder.Configuration
    .GetSection(ServiceBusOptions.SectionName)
    .Get<ServiceBusOptions>() 
    ?? throw new InvalidOperationException("ServiceBus configuration is missing.");

// Register Service Bus client as singleton
builder.Services.AddSingleton(new ServiceBusClient(serviceBusOptions.ConnectionString));

// Register message publisher
builder.Services.AddScoped<IOrderMessagePublisher, OrderMessagePublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /api/orders - Create an order and publish to Service Bus
app.MapPost("/api/orders", async (
    OrderApi.Models.CreateOrderRequest request,
    IOrderMessagePublisher messagePublisher,
    CancellationToken cancellationToken) =>
{
    // Validate request
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        return Results.BadRequest(new { error = "Customer name is required." });
    }

    if (request.TotalAmount <= 0)
    {
        return Results.BadRequest(new { error = "Total amount must be greater than zero." });
    }

    // Create the order event
    var orderEvent = new OrderCreatedEvent
    {
        OrderId = Guid.NewGuid(),
        CustomerName = request.CustomerName,
        TotalAmount = request.TotalAmount,
        CreatedAt = DateTime.UtcNow
    };

    // Publish to Service Bus
    try
    {
        await messagePublisher.PublishAsync(orderEvent, cancellationToken);

        var response = new OrderApi.Models.CreateOrderResponse
        {
            OrderId = orderEvent.OrderId,
            Message = "Order created and published to Service Bus successfully."
        };

        return Results.Created($"/api/orders/{orderEvent.OrderId}", response);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Failed to publish order to Service Bus: {ex.Message}",
            statusCode: 500);
    }
})
.WithName("CreateOrder")
.WithOpenApi()
.Accepts<OrderApi.Models.CreateOrderRequest>("application/json")
.Produces<OrderApi.Models.CreateOrderResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();
