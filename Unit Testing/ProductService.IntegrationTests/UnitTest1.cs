using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.IntegrationTests;

public class ProductApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<ProductDbContext> GetDbContextAsync()
    {
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        // Ensure a clean database for each test
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        return db;
    }

    [Fact]
    public async Task Get_Products_Returns_Seeded_Products()
    {
        // Arrange
        var db = await GetDbContextAsync();
        db.Products.AddRange(
            new Product { Name = "Seed 1", Price = 10m, StockQuantity = 5, Category = "Cat1" },
            new Product { Name = "Seed 2", Price = 20m, StockQuantity = 10, Category = "Cat2" });
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
        products!.Should().HaveCount(2);
        products.Select(p => p.Name).Should().Contain(new[] { "Seed 1", "Seed 2" });
    }

    [Fact]
    public async Task Post_Product_Creates_Product_In_Database()
    {
        // Arrange
        var db = await GetDbContextAsync();
        var newProduct = new Product
        {
            Name = "New Product",
            Price = 30m,
            StockQuantity = 3,
            Category = "Cat3"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var productsInDb = await db.Products.ToListAsync();
        productsInDb.Should().HaveCount(1);
        var created = productsInDb.Single();
        created.Name.Should().Be(newProduct.Name);
        created.Price.Should().Be(newProduct.Price);
    }

    [Fact]
    public async Task Get_Product_By_Id_Retrieves_Created_Product()
    {
        // Arrange
        var db = await GetDbContextAsync();
        var product = new Product
        {
            Name = "Existing Product",
            Price = 40m,
            StockQuantity = 4,
            Category = "Cat4"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<Product>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(product.Id);
        fetched.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task Put_Product_Updates_Product_In_Database()
    {
        // Arrange
        var db = await GetDbContextAsync();
        var product = new Product
        {
            Name = "To Update",
            Price = 50m,
            StockQuantity = 5,
            Category = "Cat5"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var updated = new Product
        {
            Name = "Updated Name",
            Price = 55m,
            StockQuantity = 6,
            Category = "Cat5"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", updated);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use a fresh DbContext instance to verify updated state
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var verifyScope = scopeFactory.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ProductDbContext>();

        var productInDb = await verifyDb.Products.SingleAsync(p => p.Id == product.Id);
        productInDb.Name.Should().Be(updated.Name);
        productInDb.Price.Should().Be(updated.Price);
        productInDb.StockQuantity.Should().Be(updated.StockQuantity);
    }

    [Fact]
    public async Task Delete_Product_Removes_Product_From_Database()
    {
        // Arrange
        var db = await GetDbContextAsync();
        var product = new Product
        {
            Name = "To Delete",
            Price = 60m,
            StockQuantity = 2,
            Category = "Cat6"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var exists = await db.Products.AnyAsync(p => p.Id == product.Id);
        exists.Should().BeFalse();
    }
}