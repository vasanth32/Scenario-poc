using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

/// <summary>
/// Compiled queries for frequently used database operations.
/// Compiled queries are pre-compiled and cached, making them faster than regular LINQ queries.
/// </summary>
public static class CompiledQueries
{
    /// <summary>
    /// Compiled query to get a product by ID.
    /// This query is compiled once and reused, making it faster than regular LINQ.
    /// </summary>
    public static readonly Func<ProductDbContext, int, Task<Product?>> GetProductByIdAsync =
        EF.CompileAsyncQuery((ProductDbContext context, int id) =>
            context.Products
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id));

    /// <summary>
    /// Compiled query to get products with pagination.
    /// Optimized for read-only operations with AsNoTracking.
    /// </summary>
    public static readonly Func<ProductDbContext, int, int, IAsyncEnumerable<Product>> GetProductsPaginatedAsync =
        EF.CompileAsyncQuery((ProductDbContext context, int skip, int take) =>
            context.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(take));

    /// <summary>
    /// Compiled query to count total products.
    /// Used for pagination metadata.
    /// </summary>
    public static readonly Func<ProductDbContext, Task<int>> GetProductCountAsync =
        EF.CompileAsyncQuery((ProductDbContext context) =>
            context.Products.Count());
}

