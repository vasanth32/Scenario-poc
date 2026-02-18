using ProductService.Models;
using ProductService.Services.Models;

namespace ProductService.Services;

public interface IProductService
{
    Task<Product> GetProductAsync(int id);
    Task<IReadOnlyList<Product>> GetAllProductsAsync();
    Task<Product> CreateProductAsync(CreateProductRequest request);
    Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
}


