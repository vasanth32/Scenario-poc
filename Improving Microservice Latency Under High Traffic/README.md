# Microservices - Product, Order, and Payment Services

Three independent .NET 8 Web API microservices demonstrating microservice architecture with health checks, structured logging, and Entity Framework Core.

## Services Overview

1. **ProductService** - Manages product catalog
2. **OrderService** - Handles order processing
3. **PaymentService** - Processes payments

## Architecture Features

Each microservice includes:
- ✅ **Minimal APIs/Controllers** - RESTful API endpoints
- ✅ **Health Check Endpoint** - `/health` for monitoring
- ✅ **Entity Framework Core** - SQLite database (for simplicity)
- ✅ **Swagger/OpenAPI** - API documentation
- ✅ **Dependency Injection** - Proper DI configuration
- ✅ **Structured Logging** - Serilog with console and file logging
- ✅ **CRUD Operations** - Basic create, read, update, delete operations

## ProductService

**Port:** Default (5000/5001)

### Endpoints

- `GET /api/products?page=1&pageSize=10` - Get all products with pagination
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create a new product
- `GET /api/products/search?query={term}` - Search products by name or description
- `GET /health` - Health check endpoint

### Database
- SQLite database: `products.db`

### Example Requests

**Create Product:**
```json
POST /api/products
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50,
  "category": "Electronics"
}
```

**Search Products:**
```
GET /api/products/search?query=laptop
```

## OrderService

**Port:** Default (5000/5001) - Configure different port in launchSettings.json

### Endpoints

- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create a new order
- `GET /health` - Health check endpoint

### Database
- SQLite database: `orders.db`

### Example Request

**Create Order:**
```json
POST /api/orders
{
  "productId": 1,
  "quantity": 2,
  "totalAmount": 1999.98,
  "customerName": "John Doe",
  "customerEmail": "john@example.com"
}
```

## PaymentService

**Port:** Default (5000/5001) - Configure different port in launchSettings.json

### Endpoints

- `POST /api/payments/process` - Process a payment
- `GET /api/payments/{id}` - Get payment by ID
- `GET /health` - Health check endpoint

### Database
- SQLite database: `payments.db`

### Example Request

**Process Payment:**
```json
POST /api/payments/process
{
  "orderId": 1,
  "amount": 1999.98,
  "paymentMethod": "CreditCard"
}
```

## Running the Services

### Option 1: Run Each Service Separately

```bash
# Terminal 1 - ProductService
cd ProductService
dotnet run

# Terminal 2 - OrderService (update port in launchSettings.json)
cd OrderService
dotnet run --urls "http://localhost:5002"

# Terminal 3 - PaymentService (update port in launchSettings.json)
cd PaymentService
dotnet run --urls "http://localhost:5003"
```

### Option 2: Update launchSettings.json

Edit `launchSettings.json` in each service to use different ports:

**ProductService:**
```json
"applicationUrl": "http://localhost:5001"
```

**OrderService:**
```json
"applicationUrl": "http://localhost:5002"
```

**PaymentService:**
```json
"applicationUrl": "http://localhost:5003"
```

## Health Checks

All services expose a health check endpoint at `/health`:

```bash
# Check ProductService health
curl http://localhost:5001/health

# Check OrderService health
curl http://localhost:5002/health

# Check PaymentService health
curl http://localhost:5003/health
```

## Logging

All services use Serilog for structured logging:

- **Console Output** - Structured JSON logs
- **File Logging** - Logs saved to `logs/` directory
  - `logs/productservice-YYYYMMDD.txt`
  - `logs/orderservice-YYYYMMDD.txt`
  - `logs/paymentservice-YYYYMMDD.txt`

Logs include:
- Service name
- Timestamp
- Log level
- Message
- Context information

## Swagger/OpenAPI

Access Swagger UI for each service:

- ProductService: `http://localhost:5001/swagger`
- OrderService: `http://localhost:5002/swagger`
- PaymentService: `http://localhost:5003/swagger`

## Database

Each service uses SQLite for simplicity:

- **ProductService**: `products.db`
- **OrderService**: `orders.db`
- **PaymentService**: `payments.db`

Databases are automatically created on first run using `EnsureCreated()`.

## Project Structure

```
Improving Microservice Latency Under High Traffic/
├── ProductService/
│   ├── Controllers/
│   │   └── ProductsController.cs
│   ├── Data/
│   │   └── ProductDbContext.cs
│   ├── Models/
│   │   └── Product.cs
│   └── Program.cs
├── OrderService/
│   ├── Controllers/
│   │   └── OrdersController.cs
│   ├── Data/
│   │   └── OrderDbContext.cs
│   ├── Models/
│   │   └── Order.cs
│   └── Program.cs
└── PaymentService/
    ├── Controllers/
    │   └── PaymentsController.cs
    ├── Data/
    │   └── PaymentDbContext.cs
    ├── Models/
    │   └── Payment.cs
    └── Program.cs
```

## Dependencies

Each service uses:
- **Microsoft.EntityFrameworkCore.Sqlite** (8.0.0) - SQLite database provider
- **Serilog.AspNetCore** (8.0.0) - Structured logging
- **Microsoft.Extensions.Diagnostics.HealthChecks** (8.0.0) - Health checks
- **Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore** (8.0.0) - EF Core health checks

## Next Steps

To improve latency under high traffic, consider:

1. **Caching** - Add Redis or in-memory caching
2. **Database Optimization** - Add indexes, connection pooling
3. **Async Operations** - Ensure all I/O operations are async
4. **Response Compression** - Enable response compression
5. **Rate Limiting** - Implement rate limiting
6. **Load Balancing** - Use load balancer for multiple instances
7. **Monitoring** - Add Application Insights or Prometheus
8. **Circuit Breaker** - Implement circuit breaker pattern for inter-service calls

