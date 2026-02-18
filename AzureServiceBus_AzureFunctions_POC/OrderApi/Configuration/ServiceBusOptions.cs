namespace OrderApi.Configuration;

/// <summary>
/// Configuration options for Azure Service Bus connection.
/// </summary>
public class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
}

