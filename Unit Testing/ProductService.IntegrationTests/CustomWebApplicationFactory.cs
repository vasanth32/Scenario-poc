using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Register an in-memory database for testing
            services.AddDbContext<ProductDbContext>(options =>
            {
                options.UseInMemoryDatabase("ProductServiceTestDb");
            });

            // Build the service provider and seed data
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ProductDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            SeedTestData(db);
        });
    }

    private static void SeedTestData(ProductDbContext db)
    {
        db.Products.RemoveRange(db.Products);

        db.Products.AddRange(
            new Product
            {
                Name = "Seeded Product 1",
                Price = 10m,
                StockQuantity = 5,
                Category = "Category1"
            },
            new Product
            {
                Name = "Seeded Product 2",
                Price = 20m,
                StockQuantity = 10,
                Category = "Category2"
            });

        db.SaveChanges();
    }
}


