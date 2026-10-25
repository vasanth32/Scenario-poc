using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Policy.Api.Authentication;
using Policy.Api.Chaos;
using Policy.Api.Data;
using Policy.Api.Repositories;
using Policy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUriValue = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUriValue))
{
    if (!Uri.TryCreate(keyVaultUriValue, UriKind.Absolute, out var keyVaultUri))
        throw new InvalidOperationException("Configuration value 'KeyVault:Uri' must be an absolute URI.");

    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

builder.Services.AddControllers();
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

    app.MapSwagger().AllowAnonymous();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "Policy.Api v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
