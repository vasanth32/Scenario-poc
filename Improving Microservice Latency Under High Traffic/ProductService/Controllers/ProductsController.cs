using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using ProductService.Data;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly CacheMetricsService _cacheMetrics;

    // Cache key prefixes
    private const string PRODUCT_CACHE_KEY_PREFIX = "product:";
    private const string PRODUCT_LIST_CACHE_KEY_PREFIX = "product:list:";
    private const string PRODUCT_SEARCH_CACHE_KEY_PREFIX = "product:search:";
    private const string CATEGORIES_CACHE_KEY = "product:categories";
    private const string PRODUCT_LIST_VERSION_KEY = "product:list:version"; // Version number for cache busting

    public ProductsController(
        ProductDbContext context,
        ILogger<ProductsController> logger,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        CacheMetricsService cacheMetrics)
    {
        _context = context;
        _logger = logger;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _cacheMetrics = cacheMetrics;
    }

    /// <summary>
    /// GET /api/products?page=1&pageSize=10
    /// Get all products with pagination
    /// Uses in-memory caching (2 min TTL) and HTTP response caching
    /// </summary>
    [HttpGet]
    [ResponseCache(CacheProfileName = "ProductListCache")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        // Get cache version - increments when products are created/updated/deleted
        var cacheVersion = _memoryCache.GetOrCreate(PRODUCT_LIST_VERSION_KEY, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1); // Version doesn't expire
            return 1;
        });

        // Cache key includes version, page and pageSize - version changes when data changes
        var cacheKey = $"{PRODUCT_LIST_CACHE_KEY_PREFIX}v{cacheVersion}:page:{page}:size:{pageSize}";

        // Try to get from in-memory cache first
        if (_memoryCache.TryGetValue(cacheKey, out List<Product>? cachedProducts))
        {
            _cacheMetrics.RecordCacheHit(cacheKey);
            _logger.LogInformation("Product list served from cache - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            return Ok(cachedProducts);
        }

        _cacheMetrics.RecordCacheMiss(cacheKey);

        // Cache miss - fetch from database
        var products = await _context.Products
            .AsNoTracking() // Read-only query - no tracking needed
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Cache the result for 2 minutes
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
            SlidingExpiration = TimeSpan.FromMinutes(1), // Reset timer if accessed
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(cacheKey, products, cacheOptions);
        _logger.LogInformation("Product list cached - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        return Ok(products);
    }

    /// <summary>
    /// GET /api/products/{id}
    /// Get product by ID
    /// Uses in-memory caching (5 min TTL) and HTTP response caching
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(CacheProfileName = "ProductDetailCache")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);

        var cacheKey = $"{PRODUCT_CACHE_KEY_PREFIX}{id}";

        // Try to get from in-memory cache first
        if (_memoryCache.TryGetValue(cacheKey, out Product? cachedProduct))
        {
            _cacheMetrics.RecordCacheHit(cacheKey);
            _logger.LogInformation("Product {ProductId} served from cache", id);
            return Ok(cachedProduct);
        }

        _cacheMetrics.RecordCacheMiss(cacheKey);

        // Cache miss - fetch from database
        var product = await _context.Products
            .AsNoTracking() // Read-only query
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound();
        }

        // Cache the result for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2), // Reset timer if accessed
            Priority = CacheItemPriority.High // Individual products are high priority
        };

        _memoryCache.Set(cacheKey, product, cacheOptions);
        _logger.LogInformation("Product {ProductId} cached", id);

        return Ok(product);
    }

    /// <summary>
    /// POST /api/products
    /// Create a new product
    /// Invalidates relevant caches after creation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product: {ProductName}", request.Name);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created with ID: {ProductId}", product.Id);

        // Cache invalidation: Clear relevant caches
        InvalidateCaches(product.Id, product.Category);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// GET /api/products/search?query={term}
    /// Search products by name or description
    /// Uses distributed cache for search results
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string query)
    {
        _logger.LogInformation("Searching products with query: {Query}", query);

        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        var cacheKey = $"{PRODUCT_SEARCH_CACHE_KEY_PREFIX}{query.ToLowerInvariant()}";

        // Try to get from distributed cache
        var cachedData = await _distributedCache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            _cacheMetrics.RecordCacheHit(cacheKey);
            _logger.LogInformation("Search results for '{Query}' served from distributed cache", query);
            var cachedProducts = JsonSerializer.Deserialize<List<Product>>(cachedData);
            return Ok(cachedProducts);
        }

        _cacheMetrics.RecordCacheMiss(cacheKey);

        // Cache miss - fetch from database
        var products = await _context.Products
            .AsNoTracking() // Read-only query
            .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
            .ToListAsync();

        // Cache the result for 5 minutes in distributed cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        var serializedProducts = JsonSerializer.Serialize(products);
        await _distributedCache.SetStringAsync(cacheKey, serializedProducts, cacheOptions);
        _logger.LogInformation("Search results for '{Query}' cached", query);

        return Ok(products);
    }

    /// <summary>
    /// GET /api/products/categories
    /// Get all product categories
    /// Uses distributed cache for categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        _logger.LogInformation("Getting product categories");

        // Try to get from distributed cache
        var cachedData = await _distributedCache.GetStringAsync(CATEGORIES_CACHE_KEY);
        if (cachedData != null)
        {
            _cacheMetrics.RecordCacheHit(CATEGORIES_CACHE_KEY);
            _logger.LogInformation("Categories served from distributed cache");
            var cachedCategories = JsonSerializer.Deserialize<List<string>>(cachedData);
            return Ok(cachedCategories);
        }

        _cacheMetrics.RecordCacheMiss(CATEGORIES_CACHE_KEY);

        // Cache miss - fetch from database
        var categories = await _context.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        // Cache for 10 minutes (categories don't change often)
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        var serializedCategories = JsonSerializer.Serialize(categories);
        await _distributedCache.SetStringAsync(CATEGORIES_CACHE_KEY, serializedCategories, cacheOptions);
        _logger.LogInformation("Categories cached");

        return Ok(categories);
    }

    /// <summary>
    /// GET /api/products/cache/metrics
    /// Get cache performance metrics
    /// </summary>
    [HttpGet("cache/metrics")]
    public ActionResult<CacheMetrics> GetCacheMetrics()
    {
        var metrics = _cacheMetrics.GetMetrics();
        _cacheMetrics.LogMetrics(); // Log to console/file
        return Ok(metrics);
    }

    /// <summary>
    /// Helper method to invalidate caches when product is created/updated/deleted
    /// </summary>
    private void InvalidateCaches(int productId, string? category = null)
    {
        _logger.LogInformation("Invalidating caches for product {ProductId}", productId);

        // Invalidate product detail cache
        var productCacheKey = $"{PRODUCT_CACHE_KEY_PREFIX}{productId}";
        _memoryCache.Remove(productCacheKey);

        // Invalidate product list caches by incrementing version number
        // This makes all existing list cache keys invalid (they use old version)
        // New requests will use new version and fetch fresh data
        var currentVersion = _memoryCache.GetOrCreate(PRODUCT_LIST_VERSION_KEY, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            return 1;
        });
        _memoryCache.Set(PRODUCT_LIST_VERSION_KEY, currentVersion + 1);
        _logger.LogInformation("Product list cache version incremented to {Version}", currentVersion + 1);

        // Invalidate categories cache if category changed
        if (!string.IsNullOrEmpty(category))
        {
            _distributedCache.RemoveAsync(CATEGORIES_CACHE_KEY);
        }

        _logger.LogInformation("Caches invalidated for product {ProductId}", productId);
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}

