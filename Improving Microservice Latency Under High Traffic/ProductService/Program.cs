using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ProductService")
    .WriteTo.Console()
    .WriteTo.File("logs/productservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// APPLICATION INSIGHTS - DISTRIBUTED TRACING & METRICS
// ============================================================================
// Application Insights picks up configuration from:
// - APPLICATIONINSIGHTS_CONNECTION_STRING environment variable, or
// - "ApplicationInsights:ConnectionString" in appsettings.json
// This will automatically track requests, dependencies, and logs.
builder.Services.AddApplicationInsightsTelemetry();

// ============================================================================
// CACHING CONFIGURATION
// ============================================================================

// 1. In-Memory Caching (IMemoryCache)
// Stores data in server memory - fast but only available on single server
// Use for: Frequently accessed data that doesn't need to be shared across servers
builder.Services.AddMemoryCache();
// Note: SizeLimit removed - if you want to limit cache size, you need to specify
// Size property on each cache entry. For POC, we'll use unlimited cache.

// 2. Distributed Caching (IDistributedCache) with Redis
// IDistributedCache is a production-ready interface/abstraction
// Using Redis for true distributed caching - shared across multiple service instances
// Get Redis connection string from configuration (defaults to localhost:6379)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? builder.Configuration["Redis:ConnectionString"] 
    ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "ProductService";
});

Log.Information("Redis distributed cache configured: {RedisConnection}", redisConnectionString);

// 3. Cache Metrics Service
// Tracks cache hit/miss rates for monitoring and pushes metrics to Application Insights
builder.Services.AddSingleton<ProductService.Services.CacheMetricsService>();

// 4. Query Performance Service
// Tracks database query performance, logs slow queries, and emits metrics
builder.Services.AddSingleton<ProductService.Services.QueryPerformanceService>();

// 3. HTTP Response Caching
// Allows browsers/CDNs to cache responses
builder.Services.AddResponseCaching();

// Configure cache profiles for HTTP response caching
builder.Services.AddControllers(options =>
{
    // Cache profile: Short cache for product lists
    options.CacheProfiles.Add("ProductListCache", new Microsoft.AspNetCore.Mvc.CacheProfile
    {
        Duration = 120, // 2 minutes
        Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any
    });
    
    // Cache profile: Longer cache for individual products
    options.CacheProfiles.Add("ProductDetailCache", new Microsoft.AspNetCore.Mvc.CacheProfile
    {
        Duration = 300, // 5 minutes
        Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any
    });
});

// ============================================================================
// DATABASE CONFIGURATION & OPTIMIZATION
// ============================================================================

// Configure Entity Framework Core with SQLite and connection pooling
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=products.db";
    
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        // Connection Pooling Configuration
        // SQLite connection pooling is handled automatically by EF Core
        // EF Core maintains a pool of DbContext instances, not raw connections
        // For SQL Server, you would configure in connection string:
        //   "Server=...;Database=...;Max Pool Size=100;Min Pool Size=10;Connection Timeout=30;"
        // SQLite uses file-based connections, so pooling works differently but is still efficient
        
        // Connection timeout (30 seconds)
        sqliteOptions.CommandTimeout(30);
    });
    
    // Enable query logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging(false) // Don't log parameter values in production
            .EnableDetailedErrors(); // More detailed error messages in development
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>();
// Note: Redis health check can be added with AspNetCore.HealthChecks.Redis package if needed

// ============================================================================
// RESPONSE COMPRESSION
// ============================================================================
// Enable response compression (Gzip/Brotli) to reduce payload size
// Automatically compresses JSON responses, reducing bandwidth usage by 60-80%
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Enable compression for HTTPS
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml" });
});

// Configure compression levels for optimal performance
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

// Response time middleware - logs slow requests (>500ms)
// Must be early in pipeline to measure entire request
app.UseMiddleware<ProductService.Middleware.ResponseTimeMiddleware>();

// Enable response compression middleware (must be before UseResponseCaching)
// Compresses responses automatically - reduces bandwidth by 60-80%
app.UseResponseCompression();

// Enable HTTP response caching middleware
app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

Log.Information("ProductService starting...");

app.Run();
