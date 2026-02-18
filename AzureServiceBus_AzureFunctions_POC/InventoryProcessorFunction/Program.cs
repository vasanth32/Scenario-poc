using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/inventory-processor-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting InventoryProcessorFunction...");

    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .ConfigureServices(services =>
        {
            // Configure Serilog as the logging provider
            services.AddSingleton(Log.Logger);
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(Log.Logger);
            });

            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        })
        .Build();

    Log.Information("InventoryProcessorFunction started successfully.");
    
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "InventoryProcessorFunction failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
