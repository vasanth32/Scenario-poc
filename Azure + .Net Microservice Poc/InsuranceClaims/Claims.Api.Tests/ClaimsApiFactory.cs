using Claims.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

// Policy.Api replacement: tests decide which policies exist, and can "delete" one mid-test.
public sealed class StubPolicyClient : IPolicyClient
{
    private readonly Dictionary<Guid, bool> _policies = new();
    public bool Down { get; set; }

    public void Add(Guid id, bool isActive) => _policies[id] = isActive;
    public void Remove(Guid id) => _policies.Remove(id);

    public Task<PolicyLookupResult> GetPolicyAsync(Guid id, CancellationToken ct)
    {
        if (Down) return Task.FromResult(new PolicyLookupResult(PolicyLookupStatus.Unavailable));

        return Task.FromResult(_policies.TryGetValue(id, out var active)
            ? new PolicyLookupResult(PolicyLookupStatus.Found, new PolicySummary(id, active))
            : new PolicyLookupResult(PolicyLookupStatus.NotFound));
    }
}

// Hosts Claims.Api against a brand-new SQL Server database that is dropped when the factory is disposed.
// Server defaults to the local SQL Express instance; override with the TEST_SQL_SERVER environment variable.
public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public StubPolicyClient Policies { get; } = new();

    public ClaimsApiFactory()
    {
        var server = Environment.GetEnvironmentVariable("TEST_SQL_SERVER") ?? @".\SQLEXPRESS";
        var database = $"ClaimsDb_Test_{Guid.NewGuid():N}";
        _connectionString = $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // Development runs MigrateAsync on startup, creating the schema.
        builder.UseSetting("ConnectionStrings:ClaimsDb", _connectionString);
        builder.ConfigureServices(services => services.AddSingleton<IPolicyClient>(Policies));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            base.Dispose(disposing);
            SqlConnection.ClearAllPools();
            using var db = new ClaimsDbContext(
                new DbContextOptionsBuilder<ClaimsDbContext>().UseSqlServer(_connectionString).Options);
            db.Database.EnsureDeleted();
            return;
        }
        base.Dispose(disposing);
    }
}
