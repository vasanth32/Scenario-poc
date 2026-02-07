# 🚀 Redis Setup Guide for Local Distributed Caching Testing

## Why Test with Redis Locally?

### Current Limitation
Your project currently uses `AddDistributedMemoryCache()` which:
- ✅ Works for single-server scenarios
- ❌ **Doesn't show true distributed behavior**
- ❌ Cache is NOT shared between multiple service instances
- ❌ Can't see the real benefits of distributed caching

### Benefits of Testing with Redis
- ✅ **See true distributed caching** - cache shared across multiple instances
- ✅ **Understand cache behavior** - see how data is serialized/stored
- ✅ **Test cache invalidation** - see how one instance clearing cache affects others
- ✅ **Learn Redis commands** - understand how distributed cache works
- ✅ **Production-ready setup** - same configuration as production

---

## 📋 Prerequisites

- Docker Desktop (recommended) OR Redis installed locally
- .NET 8 SDK
- Your ProductService project

---

## 🐳 Option 1: Redis with Docker (Easiest - Recommended)

### Step 1: Install Docker Desktop
Download from: https://www.docker.com/products/docker-desktop

### Step 2: Run Redis Container
```bash
docker run -d -p 6379:6379 --name redis-cache redis:latest
```

**Verify it's running:**
```bash
docker ps
# Should see redis-cache container running
```

**Test Redis connection:**
```bash
docker exec -it redis-cache redis-cli ping
# Should return: PONG
```

### Step 3: Install Redis Package
```bash
cd "Improving Microservice Latency Under High Traffic/ProductService"
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

### Step 4: Update Program.cs
Replace the distributed cache configuration:

```csharp
// OLD (in-memory):
// builder.Services.AddDistributedMemoryCache();

// NEW (Redis):
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "ProductService";
});
```

### Step 5: Run Your Service
```bash
dotnet run
```

**Check logs** - you should see no Redis connection errors.

---

## 💻 Option 2: Install Redis Locally (Windows)

### Step 1: Install Redis for Windows
Download from: https://github.com/microsoftarchive/redis/releases
OR use WSL2 with Redis

### Step 2-5: Same as Docker option above

---

## 🧪 Testing Distributed Caching

### Test 1: Single Instance - Basic Functionality

1. **Start ProductService:**
   ```bash
   cd ProductService
   dotnet run
   ```

2. **Test Search Endpoint:**
   ```bash
   # First request (cache miss)
   GET http://localhost:5001/api/products/search?query=laptop
   # Check logs: "Cache MISS"
   
   # Second request (cache hit)
   GET http://localhost:5001/api/products/search?query=laptop
   # Check logs: "Cache HIT" - served from Redis!
   ```

3. **Verify in Redis:**
   ```bash
   docker exec -it redis-cache redis-cli
   KEYS *
   # Should see: ProductService:product:search:laptop
   
   GET "ProductService:product:search:laptop"
   # Should see JSON data
   ```

### Test 2: Multiple Instances - True Distributed Behavior

This is where you'll see the REAL power of distributed caching!

1. **Start First Instance (Port 5001):**
   ```bash
   cd ProductService
   dotnet run --urls "http://localhost:5001"
   ```

2. **Start Second Instance (Port 5002):**
   ```bash
   # Open new terminal
   cd ProductService
   dotnet run --urls "http://localhost:5002"
   ```

3. **Test Distributed Cache Sharing:**
   ```bash
   # Request to Instance 1 - cache miss, stores in Redis
   GET http://localhost:5001/api/products/search?query=laptop
   # Logs: "Cache MISS" → "Search results cached"
   
   # Request to Instance 2 - cache HIT from Redis! (shared cache!)
   GET http://localhost:5002/api/products/search?query=laptop
   # Logs: "Cache HIT" - Instance 2 got data from Redis that Instance 1 stored!
   ```

**This proves distributed caching works!** 🎉

### Test 3: Cache Invalidation Across Instances

1. **Instance 1 caches data:**
   ```bash
   GET http://localhost:5001/api/products/categories
   # Categories cached in Redis
   ```

2. **Instance 2 gets from cache:**
   ```bash
   GET http://localhost:5002/api/products/categories
   # Cache HIT - got from Redis
   ```

3. **Create product on Instance 1 (invalidates cache):**
   ```bash
   POST http://localhost:5001/api/products
   {
     "name": "New Product",
     "category": "Electronics",
     ...
   }
   # Logs: "Invalidating caches" → Categories cache cleared
   ```

4. **Instance 2 now gets fresh data:**
   ```bash
   GET http://localhost:5002/api/products/categories
   # Cache MISS - Instance 2 sees the invalidation from Instance 1!
   ```

**This shows cache invalidation works across instances!** 🎉

---

## 🔍 Understanding Redis Data

### View All Cache Keys
```bash
docker exec -it redis-cache redis-cli
KEYS *
```

### View Specific Cache Entry
```bash
GET "ProductService:product:search:laptop"
# Returns: JSON string of products
```

### View Cache Entry TTL (Time To Live)
```bash
TTL "ProductService:product:search:laptop"
# Returns: seconds until expiration (-1 = no expiration, -2 = doesn't exist)
```

### Delete Cache Entry Manually
```bash
DEL "ProductService:product:search:laptop"
```

### Clear All Cache
```bash
FLUSHDB
# WARNING: Deletes all keys in current database
```

---

## 📊 What You'll Learn

### 1. **Serialization**
- See how objects are converted to JSON strings
- Understand why distributed cache needs serialization
- Compare with in-memory cache (no serialization needed)

### 2. **Cache Key Structure**
- See actual keys in Redis: `ProductService:product:search:laptop`
- Understand the `InstanceName` prefix
- Learn key naming conventions

### 3. **Shared State**
- Multiple instances share the same cache
- Changes in one instance affect others
- True distributed system behavior

### 4. **Cache Invalidation**
- How clearing cache in one instance affects others
- Why distributed cache is needed for load-balanced services

### 5. **Performance**
- Redis is fast (in-memory database)
- Network latency vs local memory trade-off
- When distributed cache is worth it

---

## 🐛 Troubleshooting

### Redis Connection Error
```
Error: It was not possible to connect to the redis server(s)
```

**Solution:**
1. Check Redis is running: `docker ps`
2. Check port 6379 is available: `netstat -an | findstr 6379`
3. Verify connection string: `localhost:6379`

### Port Already in Use
```
Error: bind: address already in use
```

**Solution:**
```bash
# Find process using port 6379
netstat -ano | findstr 6379

# Or use different Redis port
docker run -d -p 6380:6379 --name redis-cache redis:latest
# Update Program.cs: options.Configuration = "localhost:6380"
```

### Cache Not Working
- Check Redis logs: `docker logs redis-cache`
- Verify package installed: `dotnet list package`
- Check Program.cs configuration

---

## 📝 Quick Reference

### Redis Commands
```bash
# Connect to Redis CLI
docker exec -it redis-cache redis-cli

# View all keys
KEYS *

# Get value
GET "key-name"

# Set value
SET "key-name" "value"

# Delete key
DEL "key-name"

# Check TTL
TTL "key-name"

# View all keys matching pattern
KEYS "ProductService:*"
```

### .NET Configuration
```csharp
// Redis connection
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "ProductService";
});
```

---

## 🎯 Next Steps

1. ✅ Set up Redis locally
2. ✅ Update ProductService to use Redis
3. ✅ Test with single instance
4. ✅ Test with multiple instances (true distributed behavior)
5. ✅ Monitor Redis with `redis-cli`
6. ✅ Understand cache invalidation across instances

---

## 💡 Key Takeaways

- **Distributed cache = shared cache** across multiple servers
- **Redis = production-ready** distributed cache solution
- **Serialization required** - objects become JSON strings
- **Cache invalidation** works across all instances
- **Perfect for load-balanced** microservices

---

**Happy Testing! 🚀**

