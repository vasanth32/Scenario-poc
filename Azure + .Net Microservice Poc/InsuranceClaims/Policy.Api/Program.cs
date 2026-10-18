using Microsoft.EntityFrameworkCore;
using Policy.Api.Chaos;
using Policy.Api.Data;
using Policy.Api.Repositories;
using Policy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Override with the ConnectionStrings__PolicyDb environment variable.
var connectionString = builder.Configuration.GetConnectionString("PolicyDb")
    ?? throw new InvalidOperationException("Connection string 'PolicyDb' is not configured.");
builder.Services.AddDbContext<PolicyDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddScoped<IPolicyRepository, EfPolicyRepository>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddSingleton<ChaosState>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<ChaosMiddleware>(); // fault injection for resilience demos; never outside Development

    // Dev convenience only: create/upgrade the local database on startup.
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<PolicyDbContext>().Database.MigrateAsync();

    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Policy.Api v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
