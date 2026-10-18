using System.Text.Json.Serialization;
using Claims.Api.Data;
using Claims.Api.Repositories;
using Claims.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// Override with the ConnectionStrings__ClaimsDb environment variable.
var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDb' is not configured.");
builder.Services.AddDbContext<ClaimsDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddScoped<IClaimRepository, EfClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();

builder.Services.AddPolicyClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Dev convenience only: create/upgrade the local database on startup.
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<ClaimsDbContext>().Database.MigrateAsync();

    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Claims.Api v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can host the app in integration tests.
public partial class Program;
