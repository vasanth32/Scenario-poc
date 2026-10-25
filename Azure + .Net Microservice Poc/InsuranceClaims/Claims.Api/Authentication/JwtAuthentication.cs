using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Claims.Api.Authentication;

public static class AuthorizationPolicies
{
    public const string AdjusterOnly = "AdjusterOnly";
    public const string CustomerOnly = "CustomerOnly";
}

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "";

    [Required]
    public string Audience { get; init; } = "";

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = "";

    [Range(1, 1440)]
    public int ExpiryMinutes { get; init; } = 60;
}

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(JwtOptions.SectionName);
        var jwt = section.Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        Validator.ValidateObject(jwt, new ValidationContext(jwt), validateAllProperties: true);

        services.AddOptions<JwtOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthorizationPolicies.AdjusterOnly, policy => policy.RequireRole("Adjuster"))
            .AddPolicy(AuthorizationPolicies.CustomerOnly, policy => policy.RequireRole("Customer"));

        return services;
    }
}