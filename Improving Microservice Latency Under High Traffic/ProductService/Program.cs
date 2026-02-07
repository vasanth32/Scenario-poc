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
// Tracks cache hit/miss rates for monitoring
builder.Services.AddSingleton<ProductService.Services.CacheMetricsService>();

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

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=products.db"));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>();
// Note: Redis health check can be added with AspNetCore.HealthChecks.Redis package if needed

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
