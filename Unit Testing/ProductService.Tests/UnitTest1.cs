using FluentAssertions;
using Moq;
using ProductService.Exceptions;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using ProductService.Services.Models;

namespace ProductService.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService.Services.ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new ProductService.Services.ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetProductAsync_Returns_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            StockQuantity = 5,
            Category = "Cat"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductAsync(product.Id);

        // Assert
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetProductAsync_Throws_NotFoundException_When_Product_Does_Not_Exist()
    {
        // Arrange
        const int productId = 42;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _sut.GetProductAsync(productId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*42*");
    }

    [Fact]
    public async Task CreateProductAsync_Validates_Name_Is_Not_Empty()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            Price = 10m,
            StockQuantity = 1,
            Category = "Cat"
        };

        // Act
        var act = async () => await _sut.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task CreateProductAsync_Validates_Price_Is_Positive()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test",
            Price = 0m,
            StockQuantity = 1,
            Category = "Cat"
        };

        // Act
        var act = async () => await _sut.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*price*");
    }

    [Fact]
    public async Task CreateProductAsync_Calls_Repository_AddAsync()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Price = 15m,
            StockQuantity = 3,
            Category = "Cat"
        };

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Product>(p =>
                p.Name == request.Name &&
                p.Price == request.Price &&
                p.StockQuantity == request.StockQuantity &&
                p.Category == request.Category)),
            Times.Once);

        result.Name.Should().Be(request.Name);
        result.Price.Should().Be(request.Price);
        result.StockQuantity.Should().Be(request.StockQuantity);
        result.Category.Should().Be(request.Category);
    }

    [Fact]
    public async Task UpdateProductAsync_Validates_Inputs()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            Name = "",
            Price = -1m,
            StockQuantity = -5,
            Category = "Cat"
        };

        // Act
        var act = async () => await _sut.UpdateProductAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}