# Postman Collection Setup Guide

## 📦 Importing the Collection

### Step 1: Import Collection
1. Open Postman
2. Click **Import** button (top left)
3. Select `ProductService.postman_collection.json`
4. Click **Import**

### Step 2: Import Environment (Optional but Recommended)
1. Click **Import** button
2. Select `ProductService.postman_environment.json`
3. Click **Import**
4. Select the environment from the dropdown (top right)

### Step 3: Update Base URL
If your service runs on a different port, update the `baseUrl` variable:
- Click on **Environments** (left sidebar)
- Select **ProductService Environment**
- Update `baseUrl` to match your service URL
  - Default: `https://localhost:5001`
  - Or: `http://localhost:5000` (if using HTTP)

---

## 🧪 Testing Scenarios

### Scenario 1: Basic Cache Test

**Goal:** See the difference between cache miss and cache hit

1. **First Request (Cache Miss)**
   - Run: `GET /api/products/1`
   - **Expected:** Slower response (200-500ms) - queries database
   - Check logs: "Cache MISS"

2. **Second Request (Cache Hit)**
   - Run: `GET /api/products/1` again
   - **Expected:** Faster response (5-20ms) - from cache
   - Check logs: "Cache HIT"

**Result:** Second request should be **10-100x faster**!

---

### Scenario 2: Cache Invalidation Test

**Goal:** Verify cache is cleared when data changes

1. **Cache a Product**
   - Run: `GET /api/products/1` (caches the product)

2. **Create New Product**
   - Run: `POST /api/products` with product data
   - **Expected:** Cache is invalidated
   - Check logs: "Invalidating caches"

3. **Get Product Again**
   - Run: `GET /api/products/1`
   - **Expected:** Fresh data from database (cache was cleared)

---

### Scenario 3: Distributed Cache Test

**Goal:** Test search result caching

1. **First Search (Cache Miss)**
   - Run: `GET /api/products/search?query=laptop`
   - **Expected:** Slower - queries database
   - Check logs: "Cache MISS"

2. **Same Search (Cache Hit)**
   - Run: `GET /api/products/search?query=laptop` again
   - **Expected:** Faster - from distributed cache
   - Check logs: "Cache HIT"

---

### Scenario 4: Cache Metrics

**Goal:** Monitor cache performance

1. **Make Several Requests**
   - Run various GET requests multiple times
   - Mix cache hits and misses

2. **Check Metrics**
   - Run: `GET /api/products/cache/metrics`
   - **Expected Response:**
     ```json
     {
       "hits": 850,
       "misses": 150,
       "total": 1000,
       "hitRate": 85.0
     }
     ```
   - **Good Hit Rate:** 70-90% means cache is working well!

---

## 🚀 Using Postman Runner for Load Testing

### Step 1: Open Collection Runner
1. Click on **ProductService - Caching Tests** collection
2. Click **Run** button (top right)
3. Select requests to run

### Step 2: Configure Runner
- **Iterations:** 10-100 (number of times to run)
- **Delay:** 0ms (no delay between requests)
- Select requests:
  - ✅ Get Product by ID (Cache Test)
  - ✅ Get Products List

### Step 3: Run and Analyze
1. Click **Run ProductService**
2. Watch the results:
   - First request: Higher response time (cache miss)
   - Subsequent requests: Lower response time (cache hits)
3. Check **Average Response Time** - should decrease as cache warms up

---

## 📊 What to Look For

### Response Times
- **Cache Miss:** 200-500ms (database query)
- **Cache Hit:** 5-20ms (from memory)
- **Improvement:** 10-100x faster!

### Response Headers (HTTP Caching)
Check response headers for:
```
Cache-Control: public, max-age=300
```
This indicates HTTP response caching is working.

### Logs (Check Service Console)
Look for these log messages:
- `"Cache HIT for key: product:1"` - Cache working!
- `"Cache MISS for key: product:1"` - Database query
- `"Product 1 served from cache"` - Fast response
- `"Invalidating caches"` - Cache cleared

---

## 🎯 Quick Test Checklist

- [ ] Health check works: `GET /health`
- [ ] Get product (first time - cache miss)
- [ ] Get same product (second time - cache hit, faster!)
- [ ] Create product (invalidates cache)
- [ ] Search products (tests distributed cache)
- [ ] Get categories (tests distributed cache)
- [ ] View cache metrics (check hit rate)

---

## 💡 Tips

1. **Compare Response Times:**
   - First request vs second request
   - Should see significant improvement

2. **Check Service Logs:**
   - Watch the console where service is running
   - Look for cache hit/miss messages

3. **Use Postman Console:**
   - View → Show Postman Console
   - See detailed request/response info

4. **Test Different Scenarios:**
   - Different product IDs
   - Different search queries
   - Different pages

5. **Monitor Metrics:**
   - Run cache metrics endpoint periodically
   - Track hit rate improvement over time

---

## 🔧 Troubleshooting

### Issue: All requests are slow
**Solution:** Check if caching is enabled in `Program.cs`

### Issue: Cache metrics show 0% hit rate
**Solution:** Make sure you're running the same request twice

### Issue: Can't connect to service
**Solution:** 
- Check if service is running: `dotnet run`
- Verify port in environment variable
- Try HTTP instead of HTTPS: `http://localhost:5000`

### Issue: Cache not invalidating
**Solution:** Check logs for "Invalidating caches" message after POST

---

## 📝 Collection Structure

```
ProductService - Caching Tests
├── Health Check
│   └── Health Check
├── Products
│   ├── Get All Products (Paginated)
│   ├── Get Product by ID (Cache Test)
│   ├── Create Product
│   ├── Search Products
│   ├── Get Categories
│   └── Get Cache Metrics
├── Cache Testing Scenarios
│   ├── Scenario 1: Cache Hit Test
│   ├── Scenario 2: Cache Invalidation Test
│   └── Scenario 3: Distributed Cache Test
└── Load Testing
    └── Multiple request scenarios
```

---

**Happy Testing! 🚀**

