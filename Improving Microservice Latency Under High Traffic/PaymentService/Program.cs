using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "PaymentService")
    .WriteTo.Console()
    .WriteTo.File("logs/paymentservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// DATABASE CONFIGURATION & OPTIMIZATION
// ============================================================================

// Configure Entity Framework Core with SQLite and connection pooling
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=payments.db";
    
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        // Connection timeout (30 seconds)
        sqliteOptions.CommandTimeout(30);
    });
    
    // Enable query logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors();
    }
    
    // Query optimization: Use query splitting for better performance
    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentDbContext>();

// ============================================================================
// RESPONSE COMPRESSION
// ============================================================================
// Enable response compression (Gzip/Brotli) to reduce payload size
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml" });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Response time middleware - logs slow requests (>500ms)
app.UseMiddleware<PaymentService.Middleware.ResponseTimeMiddleware>();

// Enable response compression middleware
app.UseResponseCompression();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

Log.Information("PaymentService starting...");

app.Run();
