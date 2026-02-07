# Troubleshooting Guide

## 🔴 Common Issues and Solutions

### Issue 0: Cache Size Error (FIXED)

**Error:** `Cache entry must specify a value for Size when SizeLimit is set`

**Cause:** SizeLimit was set in Program.cs but cache entries didn't specify Size property.

**Solution:** ✅ FIXED - Removed SizeLimit from cache configuration. If you want to use SizeLimit in the future, you need to add `Size` property to each `MemoryCacheEntryOptions`.

**Status:** This has been fixed in the code.

---

### Issue 1: Connection Refused Error

**Error:** `Error: connect ECONNREFUSED`

**Cause:** Postman is trying to connect to the wrong URL/port, or the service isn't running.

**Solutions:**

1. **Check Service is Running:**
   ```bash
   cd ProductService
   dotnet run
   ```
   Look for: `Now listening on: http://localhost:XXXX`

2. **Update Postman Environment:**
   - In Postman, click on **Environments** (left sidebar)
   - Select **ProductService Environment**
   - Update `baseUrl` to match the port shown in terminal
   - Example: If terminal shows `http://localhost:5191`, set `baseUrl` to `http://localhost:5191`

3. **Check HTTP vs HTTPS:**
   - If service runs on `http://localhost:5191` (HTTP)
   - Postman should use `http://localhost:5191` (not `https://`)
   - Remove SSL certificate validation if needed

4. **Verify Port:**
   - Check `launchSettings.json` for configured ports
   - Or check terminal output when service starts
   - Update Postman environment to match

---

### Issue 2: SSL Certificate Error

**Error:** `SSL certificate problem` or `self-signed certificate`

**Solution:**
1. In Postman, go to **Settings** (gear icon)
2. Go to **General** tab
3. Turn **OFF** "SSL certificate verification"
4. Or use HTTP instead of HTTPS

---

### Issue 3: Cache Not Working

**Symptoms:** All requests are slow, cache metrics show 0% hit rate

**Solutions:**

1. **Check Caching is Enabled:**
   - Verify `Program.cs` has:
     ```csharp
     builder.Services.AddMemoryCache();
     builder.Services.AddDistributedMemoryCache();
     builder.Services.AddResponseCaching();
     ```

2. **Run Same Request Twice:**
   - First request: Cache miss (slower)
   - Second request: Cache hit (faster)
   - Must be the **exact same** request

3. **Check Logs:**
   - Look for "Cache HIT" or "Cache MISS" messages
   - If no cache messages, caching might not be working

4. **Verify Cache Keys:**
   - Different parameters = different cache keys
   - `GET /api/products/1` and `GET /api/products/2` are different
   - Must use same ID to see cache hit

---

### Issue 4: Service Won't Start

**Error:** Port already in use, or other startup errors

**Solutions:**

1. **Port Already in Use:**
   ```bash
   # Find process using port
   netstat -ano | findstr :5191
   
   # Kill the process (replace PID with actual process ID)
   taskkill /PID <PID> /F
   ```

2. **Change Port:**
   - Edit `launchSettings.json`
   - Change `applicationUrl` to different port
   - Update Postman environment

3. **Database Locked:**
   - Close any database viewers
   - Stop other instances of the service
   - Delete `products.db` and restart (will recreate)

---

### Issue 5: Cache Metrics Show 0%

**Cause:** Not enough requests, or cache not being used

**Solutions:**

1. **Make Multiple Requests:**
   - Run the same request 10+ times
   - First request = miss, rest = hits
   - Hit rate should increase

2. **Check Cache TTL:**
   - Products cached for 5 minutes
   - Lists cached for 2 minutes
   - Wait for cache to expire, then test again

3. **Verify Cache is Working:**
   - Check service logs for cache messages
   - Compare response times (first vs second request)

---

## ✅ Quick Health Check

Run these in order:

1. **Service Running?**
   ```bash
   curl http://localhost:5191/health
   ```
   Should return: `Healthy`

2. **Postman Connection?**
   - Update environment to correct URL
   - Try health check endpoint
   - Should get 200 OK

3. **Cache Working?**
   - Run GET /api/products/1 twice
   - Second should be faster
   - Check cache metrics endpoint

---

## 🔧 Quick Fixes

### Fix 1: Update Postman URL
1. Click **Environments** in Postman
2. Select **ProductService Environment**
3. Change `baseUrl` to: `http://localhost:5191` (or your actual port)
4. Save

### Fix 2: Disable SSL Verification
1. Postman → Settings (gear icon)
2. General tab
3. Turn OFF "SSL certificate verification"

### Fix 3: Check Service Port
1. Look at terminal when service starts
2. Find: `Now listening on: http://localhost:XXXX`
3. Use that port in Postman

---

## 📞 Still Having Issues?

1. **Check Service Logs:**
   - Look at terminal output
   - Check for error messages
   - Verify service started successfully

2. **Verify Database:**
   - Check if `products.db` exists
   - Service creates it automatically
   - If locked, close other connections

3. **Test with curl:**
   ```bash
   curl http://localhost:5191/health
   curl http://localhost:5191/api/products
   ```
   If curl works but Postman doesn't, it's a Postman configuration issue.

4. **Check Firewall:**
   - Windows Firewall might block connections
   - Allow .NET applications through firewall

---

## 🎯 Expected Behavior

**When Everything Works:**

1. **Health Check:**
   - `GET /health` → Returns `Healthy` (200 OK)

2. **First Request:**
   - `GET /api/products/1` → 200-500ms (database query)
   - Logs: "Cache MISS"

3. **Second Request:**
   - `GET /api/products/1` → 5-20ms (from cache)
   - Logs: "Cache HIT"

4. **Cache Metrics:**
   - `GET /api/products/cache/metrics` → Shows hits, misses, hit rate
   - Hit rate should increase with more requests

---

**Need more help? Check the service logs for detailed error messages!**

