using System.Text.Json.Serialization;
using Azure.Identity;
using Claims.Api.Authentication;
using Claims.Api.Data;
using Claims.Api.Messaging;
using Claims.Api.Repositories;
using Claims.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUriValue = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUriValue))
{
    if (!Uri.TryCreate(keyVaultUriValue, UriKind.Absolute, out var keyVaultUri))
        throw new InvalidOperationException("Configuration value 'KeyVault:Uri' must be an absolute URI.");

    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your JWT access token."
        });

    options.AddSecurityRequirement(document =>
     new Microsoft.OpenApi.OpenApiSecurityRequirement
     {
         [
             new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                 "Bearer",
                 document)
         ] = new List<string>()
     });
});
builder.Services.AddJwtAuthentication(builder.Configuration);

// Override with the ConnectionStrings__ClaimsDb environment variable.
var connectionString = builder.Configuration.GetConnectionString("ClaimsDb")
    ?? throw new InvalidOperationException("Connection string 'ClaimsDb' is not configured.");
builder.Services.AddDbContext<ClaimsDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddScoped<IClaimRepository, EfClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddClaimEventPublisher(builder.Configuration);

builder.Services.AddPolicyClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider
            .GetRequiredService<ClaimsDbContext>()
            .Database.MigrateAsync();

    app.MapSwagger().AllowAnonymous();

    app.UseSwaggerUI(o =>
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims.Api v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can host the app in integration tests.
public partial class Program;
