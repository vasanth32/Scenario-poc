using ProductService.Exceptions;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services.Models;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            throw new NotFoundException($"Product with id '{id}' was not found.");
        }

        return product;
    }

    public Task<IReadOnlyList<Product>> GetAllProductsAsync()
        => _repository.GetAllAsync();

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        ValidateProduct(request.Name, request.Price, request.StockQuantity);

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category
        };

        await _repository.AddAsync(product);
        return product;
    }

    public async Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        ValidateProduct(request.Name, request.Price, request.StockQuantity);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = request.Name;
        existing.Price = request.Price;
        existing.StockQuantity = request.StockQuantity;
        existing.Category = request.Category;

        await _repository.UpdateAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        return true;
    }

    private static void ValidateProduct(string name, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name must not be empty.", nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Product price must be positive.", nameof(price));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity must be non-negative.", nameof(stockQuantity));
        }
    }
}

