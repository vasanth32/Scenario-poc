# .NET Core Microservices Testing POCs – Cursor Prompt Script

Below are **ready-to-use Cursor AI prompts** for **comprehensive testing POCs** covering unit testing, integration testing, Moq, and microservices-specific testing scenarios.  
Run them **one-by-one** (each POC can be a separate project or separate folder).

**📊 Progress Tracking**: See `Testing_POCs_Progress.md` to track your progress for each POC.

---

## Key Learnings (By Completing All POCs)

- Write unit tests using xUnit, NUnit, or MSTest with proper test structure (Arrange-Act-Assert).
- Mock dependencies using Moq to isolate units under test.
- Test controllers, services, and repositories independently.
- Write integration tests using TestServer and WebApplicationFactory.
- Test database operations with in-memory databases and test containers.
- Test HTTP client calls and external service integrations.
- Test event-driven scenarios and message handlers.
- Test resilience patterns (retry, circuit breaker, timeout).
- Achieve high code coverage and write maintainable test code.
- Test async operations, error handling, and edge cases.

---

## POC 1 — Unit Testing Fundamentals with xUnit and Moq

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Unit Testing**: Testing individual units (methods/classes) in isolation
   - **Why**: Catch bugs early, enable refactoring, document behavior
   - **Isolation**: Each unit test should be independent and not rely on external dependencies

2. **xUnit Framework**: Popular .NET testing framework
   - `[Fact]` attribute marks a test method
   - `[Theory]` attribute for parameterized tests
   - Assertions: `Assert.Equal()`, `Assert.NotNull()`, `Assert.Throws()`

3. **Moq (Mocking)**: Creates fake objects to replace dependencies
   - **Why Mock**: Isolate the code under test, control dependencies, test edge cases
   - `Mock<T>` creates a mock object
   - `Setup()` configures mock behavior
   - `Verify()` checks if methods were called

4. **FluentAssertions**: Readable assertion library
   - `result.Should().Be(expected)` instead of `Assert.Equal(expected, result)`
   - More readable and provides better error messages

5. **AAA Pattern (Arrange-Act-Assert)**:
   - **Arrange**: Set up test data and mocks
   - **Act**: Execute the method under test
   - **Assert**: Verify the results

**Key Questions to Answer:**
- What is the difference between unit tests and integration tests?
- Why do we mock dependencies instead of using real ones?
- How does the AAA pattern make tests more readable?

### Prompt

```text
Create a .NET 8 Web API project named ProductService with:

1. Product entity: Id, Name, Price, StockQuantity, Category
2. IProductRepository interface with methods:
   - GetByIdAsync(int id)
   - GetAllAsync()
   - AddAsync(Product product)
   - UpdateAsync(Product product)
   - DeleteAsync(int id)
3. ProductRepository class implementing IProductRepository (using in-memory list for now)
4. IProductService interface with methods:
   - GetProductAsync(int id)
   - GetAllProductsAsync()
   - CreateProductAsync(CreateProductRequest request)
   - UpdateProductAsync(int id, UpdateProductRequest request)
   - DeleteProductAsync(int id)
5. ProductService class implementing IProductService with business logic:
   - Validate product name is not empty
   - Validate price is positive
   - Validate stock quantity is non-negative
   - Throw ArgumentException for invalid inputs
6. ProductController with CRUD endpoints

Then create a test project ProductService.Tests:
- Add xUnit, Moq, FluentAssertions packages
- Write unit tests for ProductService:
  - Test GetProductAsync returns product when exists
  - Test GetProductAsync throws NotFoundException when product doesn't exist
  - Test CreateProductAsync validates name is not empty
  - Test CreateProductAsync validates price is positive
  - Test CreateProductAsync calls repository AddAsync
  - Test UpdateProductAsync validates inputs
  - Use Moq to mock IProductRepository
  - Use FluentAssertions for assertions
  - Follow AAA pattern (Arrange-Act-Assert)

After implementation, explain:
1. Why we mock IProductRepository instead of using the real implementation
2. How the AAA pattern is applied in each test
3. What FluentAssertions provides over standard Assert methods
4. How to verify that a mocked method was called with specific parameters
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created ProductService project with entities and interfaces
- [ ] Implemented ProductService with business logic
- [ ] Created ProductService.Tests project
- [ ] Installed xUnit, Moq, FluentAssertions packages
- [ ] Wrote unit tests for GetProductAsync (success and failure cases)
- [ ] Wrote unit tests for CreateProductAsync validation
- [ ] Wrote unit tests for UpdateProductAsync
- [ ] All tests passing
- [ ] Understood why mocking is used
- [ ] Understood AAA pattern application

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 2 — Testing Controllers and HTTP Endpoints

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Controller Testing**: Testing ASP.NET Core controllers
   - Controllers handle HTTP requests and return responses
   - Test HTTP status codes, response models, and action results
   - Mock the service layer to isolate controller logic

2. **Action Results**: Types returned by controller actions
   - `Ok()` → 200 OK
   - `CreatedAtAction()` → 201 Created
   - `NotFound()` → 404 Not Found
   - `BadRequest()` → 400 Bad Request
   - `NoContent()` → 204 No Content

3. **Model Validation**: Data Annotations validate input
   - `[Required]`, `[Range]`, `[EmailAddress]` attributes
   - `ModelState.IsValid` checks validation
   - Invalid models return 400 BadRequest

4. **Testing Action Results**:
   - Cast result to specific type (e.g., `OkObjectResult`)
   - Assert status code: `Assert.Equal(200, result.StatusCode)`
   - Assert response data: `Assert.NotNull(result.Value)`

**Key Questions to Answer:**
- Why do we test controllers separately from services?
- What HTTP status codes should each operation return?
- How do we test model validation in controllers?

### Prompt

```text
Extend ProductService from POC 1:

1. Add ProductController with full CRUD endpoints
2. Add proper HTTP status codes (200, 201, 404, 400)
3. Add input validation using Data Annotations
4. Add error handling middleware

Create ProductService.Tests project with:

1. Unit tests for ProductController:
   - Test GET /api/products returns 200 OK with products
   - Test GET /api/products/{id} returns 200 OK with product
   - Test GET /api/products/{id} returns 404 when not found
   - Test POST /api/products returns 201 Created
   - Test POST /api/products returns 400 BadRequest for invalid input
   - Test PUT /api/products/{id} returns 200 OK
   - Test DELETE /api/products/{id} returns 204 NoContent
   - Use Moq to mock IProductService
   - Test controller action results, status codes, and response models
   - Use ObjectResult, CreatedAtActionResult assertions

After implementation, explain:
1. How to test different action result types (Ok, NotFound, BadRequest)
2. Why we mock IProductService in controller tests
3. How to verify HTTP status codes in controller tests
4. How model validation is tested in controllers
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Extended ProductController with CRUD endpoints
- [ ] Added proper HTTP status codes
- [ ] Added input validation with Data Annotations
- [ ] Wrote tests for GET endpoints (200, 404)
- [ ] Wrote tests for POST endpoint (201, 400)
- [ ] Wrote tests for PUT endpoint (200)
- [ ] Wrote tests for DELETE endpoint (204)
- [ ] All tests passing
- [ ] Understood action result types
- [ ] Understood controller testing approach

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 3 — Integration Testing with WebApplicationFactory

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Integration Testing**: Testing multiple components together
   - Tests the full HTTP pipeline (routing, middleware, controllers, services, database)
   - More realistic than unit tests but slower
   - Catches issues that unit tests miss (routing, model binding, middleware)

2. **WebApplicationFactory<TProgram>**: Creates a test server
   - Hosts your application in-memory for testing
   - Provides `HttpClient` to make real HTTP requests
   - Can override configuration (e.g., use in-memory database)

3. **In-Memory Database**: Fast database for testing
   - `UseInMemoryDatabase()` creates a temporary database
   - Faster than real database, but has limitations
   - Perfect for integration tests

4. **Test Isolation**: Each test should be independent
   - Use unique database names or clean up after each test
   - Seed test data in factory or test setup

**Key Questions to Answer:**
- What's the difference between unit tests and integration tests?
- Why use WebApplicationFactory instead of starting the app manually?
- When should you use in-memory database vs real database?

### Prompt

```text
Extend ProductService to use Entity Framework Core with SQL Server:

1. Add ProductDbContext with Product DbSet
2. Implement ProductRepository using EF Core
3. Configure dependency injection in Program.cs
4. Add database migrations

Create ProductService.IntegrationTests project:

1. Add Microsoft.AspNetCore.Mvc.Testing, xUnit, FluentAssertions packages
2. Create CustomWebApplicationFactory<TProgram>:
   - Override ConfigureWebHost to use in-memory database (UseInMemoryDatabase)
   - Seed test data in factory
3. Write integration tests:
   - Test GET /api/products returns seeded products
   - Test POST /api/products creates product in database
   - Test GET /api/products/{id} retrieves created product
   - Test PUT /api/products/{id} updates product in database
   - Test DELETE /api/products/{id} removes product from database
   - Use HttpClient from factory to make HTTP requests
   - Verify database state after operations
   - Test with real HTTP pipeline (middleware, routing, model binding)

After implementation, explain:
1. How WebApplicationFactory creates a test server
2. Why we override ConfigureWebHost to use in-memory database
3. How integration tests differ from unit tests
4. What parts of the application are tested in integration tests vs unit tests
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Added ProductDbContext with EF Core
- [ ] Implemented ProductRepository with EF Core
- [ ] Configured dependency injection
- [ ] Created ProductService.IntegrationTests project
- [ ] Created CustomWebApplicationFactory
- [ ] Configured in-memory database
- [ ] Seeded test data
- [ ] Wrote integration tests for all CRUD operations
- [ ] Verified database state in tests
- [ ] All tests passing
- [ ] Understood WebApplicationFactory concept
- [ ] Understood integration vs unit testing

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 4 — Testing Database Operations with In-Memory Database

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Repository Pattern**: Abstraction layer over data access
   - Separates business logic from data access
   - Makes testing easier (can mock repository)
   - Provides consistent interface for data operations

2. **Testing Repositories**: Test data access layer directly
   - Use in-memory database for fast tests
   - Test CRUD operations
   - Test complex queries and filters
   - Verify relationships (e.g., Order with OrderItems)

3. **DbContextOptionsBuilder**: Configures EF Core DbContext
   - `UseInMemoryDatabase()` for testing
   - `UseSqlServer()` for production
   - Can create new DbContext for each test

4. **Test Isolation**: Each test gets fresh database
   - Create new DbContext in test setup
   - Use unique database names: `UseInMemoryDatabase(Guid.NewGuid().ToString())`
   - Ensures tests don't interfere with each other

**Key Questions to Answer:**
- Why test repositories separately from services?
- How does in-memory database help with testing?
- Why create a fresh DbContext for each test?

### Prompt

```text
Create OrderService with:

1. Order entity: Id, CustomerId, OrderDate, TotalAmount, Status
2. OrderItem entity: Id, OrderId, ProductId, Quantity, Price
3. OrderDbContext with Orders and OrderItems DbSets
4. IOrderRepository with methods for CRUD operations
5. OrderRepository using EF Core
6. OrderService with business logic:
   - CreateOrderAsync calculates total from items
   - UpdateOrderStatusAsync validates status transitions
   - GetOrdersByCustomerAsync filters by customer

Create OrderService.Tests project:

1. Unit tests for OrderRepository:
   - Use DbContextOptionsBuilder with UseInMemoryDatabase
   - Test AddAsync creates order in database
   - Test GetByIdAsync retrieves order with items
   - Test UpdateAsync modifies order
   - Test DeleteAsync removes order
   - Test GetOrdersByCustomerAsync filters correctly
   - Create fresh DbContext for each test
   - Clean up database after each test
2. Integration tests for OrderService:
   - Test CreateOrderAsync calculates total correctly
   - Test order items are saved with order
   - Test status transitions are validated
   - Use in-memory database for isolation

After implementation, explain:
1. How to set up in-memory database for repository tests
2. Why we create a fresh DbContext for each test
3. How to test relationships (Order with OrderItems)
4. How repository tests differ from service tests
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created OrderService with entities
- [ ] Created OrderDbContext
- [ ] Implemented IOrderRepository and OrderRepository
- [ ] Implemented OrderService with business logic
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for OrderRepository CRUD
- [ ] Wrote tests for relationships (Order with Items)
- [ ] Wrote tests for filtering (GetOrdersByCustomerAsync)
- [ ] Wrote integration tests for OrderService
- [ ] All tests passing
- [ ] Understood repository pattern
- [ ] Understood in-memory database setup

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 5 — Testing HTTP Client Calls and External Services

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **HttpClient**: Used to make HTTP requests to external APIs
   - Configure base URL, timeouts, headers
   - Serialize/deserialize JSON
   - Handle errors and exceptions

2. **Mocking External Services**: Can't call real external APIs in tests
   - Mock the client interface (IPaymentGatewayClient)
   - Control responses (success, failure, timeout)
   - Verify requests were made correctly

3. **WireMock.NET**: Mock HTTP server for integration tests
   - Simulates external API
   - Can test real HTTP calls without external dependency
   - Configure responses, delays, errors

4. **Testing HTTP Client Configuration**:
   - Test base URL is correct
   - Test timeout settings
   - Test retry policies
   - Test error handling

**Key Questions to Answer:**
- Why mock external services instead of calling them?
- What's the difference between mocking and using WireMock?
- How do you test timeout and error scenarios?

### Prompt

```text
Create PaymentService that calls external Payment Gateway API:

1. IPaymentGatewayClient interface:
   - ProcessPaymentAsync(PaymentRequest request)
   - RefundPaymentAsync(string transactionId)
2. PaymentGatewayClient class using HttpClient:
   - Base URL from configuration
   - JSON serialization
   - Error handling
3. PaymentService that uses IPaymentGatewayClient
4. PaymentController with endpoints

Create PaymentService.Tests project:

1. Unit tests for PaymentService:
   - Mock IPaymentGatewayClient using Moq
   - Test ProcessPaymentAsync calls gateway client
   - Test handles gateway exceptions
   - Test retry logic (if implemented)
2. Integration tests with HttpMock:
   - Use HttpClientMock or WireMock.NET
   - Mock external API responses
   - Test 200 OK response
   - Test 500 Internal Server Error response
   - Test timeout scenarios
   - Verify request payloads sent to external API
3. Test HttpClient configuration:
   - Test base URL configuration
   - Test timeout settings
   - Test retry policies (Polly)

After implementation, explain:
1. Why we create an interface for HttpClient (IPaymentGatewayClient)
2. How to mock async methods with Moq
3. When to use WireMock vs Moq for testing HTTP calls
4. How to test timeout and error scenarios
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created PaymentService with IPaymentGatewayClient interface
- [ ] Implemented PaymentGatewayClient with HttpClient
- [ ] Created PaymentService and PaymentController
- [ ] Created PaymentService.Tests project
- [ ] Wrote unit tests with Moq for PaymentService
- [ ] Installed and configured WireMock.NET
- [ ] Wrote integration tests with WireMock
- [ ] Tested success scenarios (200 OK)
- [ ] Tested error scenarios (500, timeout)
- [ ] Tested HttpClient configuration
- [ ] All tests passing
- [ ] Understood HTTP client testing
- [ ] Understood mocking external services

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 6 — Testing Microservices Communication

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Microservices Communication**: Services call each other via HTTP
   - OrderService → InventoryService (check stock)
   - OrderService → PaymentService (process payment)
   - Need to test both success and failure scenarios

2. **Service Client Interfaces**: Abstraction for service-to-service calls
   - `IInventoryServiceClient`, `IPaymentServiceClient`
   - Can be mocked in unit tests
   - Real implementation uses HttpClient

3. **TestServer for Multiple Services**: Test multiple services together
   - Create TestServer for each service
   - Test real HTTP communication
   - Verify end-to-end flow

4. **Contract Testing**: Verify API contracts between services
   - Request/response schemas match
   - Prevents breaking changes
   - Ensures services can communicate

**Key Questions to Answer:**
- How do you test communication between multiple microservices?
- What's the difference between unit and integration tests for microservices?
- Why is contract testing important?

### Prompt

```text
Create two microservices:

1. OrderService:
   - POST /api/orders creates order
   - Calls InventoryService to check stock
   - Calls PaymentService to process payment
2. InventoryService:
   - GET /api/inventory/{productId}/stock
   - POST /api/inventory/reserve
3. PaymentService:
   - POST /api/payments/process

Create OrderService.Tests project:

1. Unit tests for OrderService:
   - Mock IInventoryServiceClient and IPaymentServiceClient
   - Test order creation when inventory available
   - Test order creation fails when inventory unavailable
   - Test order creation fails when payment fails
   - Test compensation logic (rollback inventory)
2. Integration tests:
   - Use TestServer for InventoryService and PaymentService
   - Test real HTTP communication between services
   - Test service discovery (if implemented)
   - Test circuit breaker behavior
   - Test timeout handling
3. Contract tests:
   - Test API contracts between services
   - Verify request/response schemas

After implementation, explain:
1. How to test microservices that call other microservices
2. When to use TestServer vs mocking in microservices tests
3. What contract testing ensures
4. How to test compensation/rollback logic
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created OrderService, InventoryService, PaymentService
- [ ] Created service client interfaces
- [ ] Implemented service-to-service communication
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests with mocked service clients
- [ ] Wrote integration tests with TestServer
- [ ] Tested success scenarios
- [ ] Tested failure scenarios and compensation
- [ ] Wrote contract tests
- [ ] All tests passing
- [ ] Understood microservices testing approach
- [ ] Understood contract testing

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 7 — Testing Event-Driven Scenarios

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Event-Driven Architecture**: Services communicate via events
   - Publisher publishes events (OrderCreatedEvent)
   - Subscribers consume events and react
   - Loose coupling between services

2. **Event Publisher**: Publishes events to message bus
   - `IEventPublisher` interface
   - Can be mocked in unit tests
   - Real implementation uses RabbitMQ/Azure Service Bus

3. **Event Handlers**: Process events when received
   - Subscribe to specific event types
   - Handle business logic
   - Can publish new events

4. **In-Memory Message Bus**: For testing event-driven flows
   - Simulates message bus without external dependency
   - Test event publishing and consumption
   - Verify event data and ordering

**Key Questions to Answer:**
- How do you test event publishing?
- How do you test event handlers?
- What's the difference between in-memory and real message bus for testing?

### Prompt

```text
Create Event-Driven Order Processing:

1. OrderService publishes OrderCreatedEvent
2. InventoryService subscribes and reserves stock
3. PaymentService subscribes and processes payment
4. NotificationService subscribes and sends email

Use RabbitMQ or in-memory message bus for POC.

Create OrderService.Tests project:

1. Unit tests for event publishers:
   - Mock IEventPublisher
   - Test OrderCreatedEvent is published with correct data
   - Test event serialization
2. Integration tests for event handlers:
   - Test InventoryServiceHandler processes OrderCreatedEvent
   - Test PaymentServiceHandler processes OrderCreatedEvent
   - Use in-memory message bus for testing
   - Verify events are consumed correctly
   - Test event ordering and idempotency
3. Test event-driven workflows:
   - Test complete order flow through events
   - Test compensation events (OrderCancelledEvent)
   - Test event replay scenarios

After implementation, explain:
1. How to test event publishing with mocks
2. How to test event handlers with in-memory message bus
3. Why event ordering and idempotency matter
4. How to test event-driven workflows end-to-end
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created event-driven architecture
- [ ] Created IEventPublisher interface
- [ ] Implemented event publishers
- [ ] Implemented event handlers
- [ ] Set up in-memory message bus for testing
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for event publishers
- [ ] Wrote integration tests for event handlers
- [ ] Tested event-driven workflows
- [ ] Tested compensation events
- [ ] All tests passing
- [ ] Understood event-driven testing
- [ ] Understood event ordering and idempotency

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 8 — Testing Resilience Patterns (Polly)

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Resilience Patterns**: Handle failures gracefully
   - **Retry**: Retry failed operations
   - **Circuit Breaker**: Stop calling failing service
   - **Timeout**: Fail fast if service is slow

2. **Polly**: .NET resilience library
   - Configures retry, circuit breaker, timeout policies
   - Wraps service calls with policies
   - Handles exceptions and retries

3. **Testing Resilience**: Verify policies work correctly
   - Mock service to fail/succeed
   - Verify retry attempts
   - Verify circuit breaker state changes
   - Verify timeout behavior

4. **Circuit Breaker States**:
   - **Closed**: Normal operation
   - **Open**: Failing, blocks calls
   - **Half-Open**: Testing if service recovered

**Key Questions to Answer:**
- Why use retry, circuit breaker, and timeout?
- How do you test retry logic?
- How do you test circuit breaker state transitions?

### Prompt

```text
Create OrderService with resilience patterns:

1. Implement retry policy for PaymentService calls (3 retries, exponential backoff)
2. Implement circuit breaker (opens after 5 failures, closes after 30 seconds)
3. Implement timeout policy (5 seconds)
4. Use Polly for resilience policies

Create OrderService.Tests project:

1. Unit tests for retry logic:
   - Mock IPaymentServiceClient to fail N times then succeed
   - Verify retry attempts are made
   - Verify exponential backoff timing
   - Test retry exhaustion scenario
2. Unit tests for circuit breaker:
   - Mock IPaymentServiceClient to fail consistently
   - Verify circuit opens after threshold
   - Verify circuit blocks calls when open
   - Verify circuit closes after timeout
   - Test half-open state
3. Unit tests for timeout:
   - Mock IPaymentServiceClient to delay response
   - Verify timeout exception is thrown
   - Verify timeout duration
4. Integration tests:
   - Test resilience policies with real HTTP calls
   - Use TestServer to simulate slow/failing services
   - Verify policies work together

After implementation, explain:
1. How to test retry logic with mocks
2. How to verify circuit breaker state transitions
3. How to test timeout scenarios
4. Why resilience patterns are important in microservices
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Installed Polly package
- [ ] Implemented retry policy
- [ ] Implemented circuit breaker policy
- [ ] Implemented timeout policy
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for retry logic
- [ ] Wrote unit tests for circuit breaker
- [ ] Wrote unit tests for timeout
- [ ] Wrote integration tests for resilience patterns
- [ ] All tests passing
- [ ] Understood retry pattern
- [ ] Understood circuit breaker pattern
- [ ] Understood timeout pattern

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 9 — Testing Async Operations and Concurrency

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Concurrency**: Multiple operations happening simultaneously
   - Race conditions: Two operations modify same data
   - Need atomic operations to prevent data corruption

2. **Optimistic Concurrency**: Assume no conflicts, detect if they occur
   - Use RowVersion/timestamp column
   - Throws `DbUpdateConcurrencyException` on conflict
   - Retry on conflict

3. **Pessimistic Locking**: Lock data during operation
   - Prevents concurrent modifications
   - Can cause deadlocks
   - Slower but safer

4. **Testing Concurrency**: Verify correct behavior under load
   - Use `Task.WhenAll()` for parallel requests
   - Verify no data corruption
   - Test exception handling

**Key Questions to Answer:**
- What are race conditions and how do you prevent them?
- What's the difference between optimistic and pessimistic concurrency?
- How do you test concurrent operations?

### Prompt

```text
Create InventoryService with concurrent operations:

1. ReserveStockAsync method that:
   - Checks available stock
   - Reserves stock atomically
   - Handles concurrent requests
2. Use optimistic concurrency (RowVersion) or pessimistic locking
3. Handle race conditions

Create InventoryService.Tests project:

1. Unit tests for concurrent operations:
   - Test ReserveStockAsync with single request
   - Test ReserveStockAsync with concurrent requests (Task.WhenAll)
   - Verify stock is reserved correctly
   - Verify no overselling occurs
   - Test DbUpdateConcurrencyException handling
2. Integration tests:
   - Test concurrent API calls
   - Use parallel HTTP requests
   - Verify database consistency
   - Test deadlock scenarios
3. Load tests:
   - Test with high concurrency (100+ requests)
   - Measure performance
   - Verify correctness under load

After implementation, explain:
1. How to test concurrent operations with Task.WhenAll
2. How optimistic concurrency prevents race conditions
3. How to handle DbUpdateConcurrencyException
4. Why testing concurrency is important
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created InventoryService with ReserveStockAsync
- [ ] Implemented optimistic concurrency (RowVersion)
- [ ] Created InventoryService.Tests project
- [ ] Wrote unit tests for single request
- [ ] Wrote unit tests for concurrent requests
- [ ] Tested DbUpdateConcurrencyException handling
- [ ] Wrote integration tests for concurrent API calls
- [ ] Wrote load tests with high concurrency
- [ ] Verified no overselling occurs
- [ ] All tests passing
- [ ] Understood concurrency concepts
- [ ] Understood optimistic vs pessimistic locking

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 10 — Testing Authentication and Authorization

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Authentication**: Verifying who the user is
   - JWT (JSON Web Token) contains user identity
   - Token is validated by middleware
   - Unauthenticated requests return 401 Unauthorized

2. **Authorization**: Verifying what the user can do
   - Roles: Admin, User, etc.
   - Policies: Custom authorization rules
   - Unauthorized requests return 403 Forbidden

3. **Testing Authentication**: Test with valid/invalid tokens
   - Create test JWT tokens
   - Test token validation
   - Test expired/invalid tokens

4. **Testing Authorization**: Test role and policy enforcement
   - Test different user roles
   - Test policy-based authorization
   - Verify 401/403 responses

**Key Questions to Answer:**
- What's the difference between authentication and authorization?
- How do you create test JWT tokens?
- How do you test role-based authorization?

### Prompt

```text
Create OrderService with JWT authentication:

1. Add authentication middleware (JWT Bearer)
2. Add authorization policies (Admin, User roles)
3. Protect endpoints with [Authorize] attribute
4. Add role-based access control

Create OrderService.Tests project:

1. Unit tests for authorization:
   - Test [Authorize] attribute behavior
   - Test role-based authorization
   - Test policy-based authorization
2. Integration tests with authentication:
   - Create test JWT tokens
   - Test authenticated requests
   - Test unauthorized requests (401)
   - Test forbidden requests (403)
   - Test different user roles
3. Test authentication middleware:
   - Test token validation
   - Test expired tokens
   - Test invalid tokens
   - Use TestServer with authentication

After implementation, explain:
1. How to create test JWT tokens for testing
2. How to test authentication in integration tests
3. How to test role-based and policy-based authorization
4. What HTTP status codes to expect for auth failures
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Added JWT authentication middleware
- [ ] Added authorization policies
- [ ] Protected endpoints with [Authorize]
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for authorization
- [ ] Created test JWT token helper
- [ ] Wrote integration tests with authentication
- [ ] Tested authenticated requests
- [ ] Tested unauthorized requests (401)
- [ ] Tested forbidden requests (403)
- [ ] Tested different user roles
- [ ] All tests passing
- [ ] Understood authentication vs authorization
- [ ] Understood JWT token testing

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 11 — Testing Validation and Error Handling

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **FluentValidation**: Fluent API for validation rules
   - More powerful than Data Annotations
   - Complex validation logic
   - Custom error messages

2. **Global Exception Handling**: Catch all exceptions
   - Middleware catches unhandled exceptions
   - Returns consistent error format (ProblemDetails)
   - Logs exceptions

3. **ProblemDetails**: Standard error response format (RFC 7807)
   - `type`, `title`, `status`, `detail`, `instance`
   - Consistent error format across API

4. **Testing Error Handling**: Test all error scenarios
   - Validation errors (400)
   - Not found errors (404)
   - Server errors (500)
   - Verify error format and logging

**Key Questions to Answer:**
- Why use FluentValidation over Data Annotations?
- What is ProblemDetails and why use it?
- How do you test exception handling middleware?

### Prompt

```text
Create ProductService with comprehensive validation:

1. Add FluentValidation for request validation
2. Add custom validation attributes
3. Add global exception handling middleware
4. Add custom exceptions (NotFoundException, ValidationException)
5. Return proper error responses (ProblemDetails)

Create ProductService.Tests project:

1. Unit tests for validators:
   - Test FluentValidation rules
   - Test custom validators
   - Test validation error messages
2. Integration tests for error handling:
   - Test 400 BadRequest for validation errors
   - Test 404 NotFound for missing resources
   - Test 500 InternalServerError for exceptions
   - Test error response format (ProblemDetails)
   - Test exception logging
3. Test error scenarios:
   - Test database connection failures
   - Test external service failures
   - Test timeout scenarios
   - Test malformed requests

After implementation, explain:
1. How to test FluentValidation rules
2. How to test global exception handling middleware
3. What ProblemDetails provides over custom error responses
4. How to test different error scenarios
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Added FluentValidation
- [ ] Created custom validators
- [ ] Added global exception handling middleware
- [ ] Created custom exceptions
- [ ] Configured ProblemDetails responses
- [ ] Created ProductService.Tests project
- [ ] Wrote unit tests for validators
- [ ] Wrote integration tests for error handling
- [ ] Tested validation errors (400)
- [ ] Tested not found errors (404)
- [ ] Tested server errors (500)
- [ ] Verified ProblemDetails format
- [ ] All tests passing
- [ ] Understood FluentValidation
- [ ] Understood error handling patterns

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 12 — Testing CQRS Pattern

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **CQRS (Command Query Responsibility Segregation)**:
   - **Commands**: Change state (Create, Update, Delete)
   - **Queries**: Read data (Get, List)
   - Separate read and write models

2. **Command Handlers**: Process commands
   - Validate command
   - Execute business logic
   - Update write model

3. **Query Handlers**: Process queries
   - Read from read model
   - Map to DTOs
   - Return results

4. **Testing CQRS**: Test handlers independently
   - Mock dependencies
   - Test command validation
   - Test query mapping

**Key Questions to Answer:**
- What is CQRS and why use it?
- How do commands differ from queries?
- How do you test command and query handlers?

### Prompt

```text
Create OrderService with CQRS pattern:

1. Commands:
   - CreateOrderCommand
   - UpdateOrderCommand
   - CancelOrderCommand
2. Queries:
   - GetOrderQuery
   - GetOrdersByCustomerQuery
3. Command handlers and query handlers
4. Separate read and write models

Create OrderService.Tests project:

1. Unit tests for command handlers:
   - Test CreateOrderCommandHandler
   - Test UpdateOrderCommandHandler
   - Test CancelOrderCommandHandler
   - Mock dependencies
   - Test command validation
2. Unit tests for query handlers:
   - Test GetOrderQueryHandler
   - Test GetOrdersByCustomerQueryHandler
   - Test query results mapping
3. Integration tests:
   - Test complete CQRS flow
   - Test command-query separation
   - Test read model updates

After implementation, explain:
1. What CQRS pattern provides
2. How to test command handlers
3. How to test query handlers
4. Why separate read and write models
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created CQRS structure (Commands, Queries, Handlers)
- [ ] Implemented command handlers
- [ ] Implemented query handlers
- [ ] Separated read and write models
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for command handlers
- [ ] Wrote unit tests for query handlers
- [ ] Tested command validation
- [ ] Tested query mapping
- [ ] Wrote integration tests for CQRS flow
- [ ] All tests passing
- [ ] Understood CQRS pattern
- [ ] Understood command-query separation

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 13 — Testing Repository Pattern and Unit of Work

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Repository Pattern**: Abstraction over data access
   - Hides database implementation
   - Makes testing easier
   - Provides consistent interface

2. **Unit of Work Pattern**: Manages transactions
   - Groups multiple operations
   - Commits or rolls back together
   - Ensures data consistency

3. **Testing Unit of Work**: Test transaction behavior
   - Test commit (SaveChangesAsync)
   - Test rollback (exception handling)
   - Test multiple repositories in transaction

4. **Transaction Isolation**: Test concurrent transactions
   - Verify data consistency
   - Test isolation levels
   - Test deadlock scenarios

**Key Questions to Answer:**
- What is the Unit of Work pattern?
- How do repositories and Unit of Work work together?
- How do you test transaction rollback?

### Prompt

```text
Create OrderService with Repository and Unit of Work patterns:

1. IUnitOfWork interface with:
   - IOrderRepository Orders
   - IProductRepository Products
   - SaveChangesAsync()
   - BeginTransactionAsync()
2. UnitOfWork implementation using DbContext
3. Repository implementations

Create OrderService.Tests project:

1. Unit tests for repositories:
   - Test repository CRUD operations
   - Use in-memory database
   - Test query methods
2. Unit tests for Unit of Work:
   - Test SaveChangesAsync commits changes
   - Test transaction rollback
   - Test multiple repositories in same transaction
3. Integration tests:
   - Test Unit of Work with real database
   - Test transaction isolation
   - Test concurrent transactions

After implementation, explain:
1. What the Unit of Work pattern provides
2. How to test transaction commit and rollback
3. How repositories and Unit of Work work together
4. Why test transaction isolation
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created IUnitOfWork interface
- [ ] Implemented UnitOfWork with DbContext
- [ ] Created repository interfaces and implementations
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for repositories
- [ ] Wrote unit tests for Unit of Work
- [ ] Tested SaveChangesAsync (commit)
- [ ] Tested transaction rollback
- [ ] Tested multiple repositories in transaction
- [ ] Wrote integration tests for transactions
- [ ] All tests passing
- [ ] Understood Repository pattern
- [ ] Understood Unit of Work pattern

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 14 — Testing Background Services and Hosted Services

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **IHostedService**: Long-running background service
   - Runs in background
   - Implements `StartAsync()` and `StopAsync()`
   - Used for polling, processing, etc.

2. **Cancellation Tokens**: Graceful shutdown
   - `CancellationToken` signals cancellation
   - Allows graceful shutdown
   - Important for background services

3. **Testing Background Services**: Test async execution
   - Mock dependencies
   - Test service logic
   - Test cancellation

4. **Job Scheduling**: Scheduled tasks (Hangfire/Quartz)
   - Recurring jobs
   - One-time jobs
   - Test job execution and retry

**Key Questions to Answer:**
- What are background services used for?
- How do you test long-running services?
- Why are cancellation tokens important?

### Prompt

```text
Create OrderService with background processing:

1. OrderProcessingService (IHostedService):
   - Polls database for pending orders
   - Processes orders in background
   - Publishes events
2. Scheduled tasks (Hangfire or Quartz.NET)
3. Background job processing

Create OrderService.Tests project:

1. Unit tests for background services:
   - Mock dependencies
   - Test service logic
   - Test cancellation tokens
2. Integration tests:
   - Test background service execution
   - Test job scheduling
   - Test job retry logic
   - Use TestServer with hosted services
3. Test service lifecycle:
   - Test service startup
   - Test service shutdown
   - Test graceful shutdown

After implementation, explain:
1. How to test IHostedService
2. How to test cancellation tokens
3. How to test job scheduling
4. Why test service lifecycle
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Created OrderProcessingService (IHostedService)
- [ ] Implemented background processing logic
- [ ] Added job scheduling (Hangfire/Quartz)
- [ ] Created OrderService.Tests project
- [ ] Wrote unit tests for background service
- [ ] Tested cancellation tokens
- [ ] Wrote integration tests for service execution
- [ ] Tested job scheduling
- [ ] Tested job retry logic
- [ ] Tested service lifecycle (startup/shutdown)
- [ ] All tests passing
- [ ] Understood IHostedService
- [ ] Understood background service testing

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## POC 15 — Complete Microservice Testing Suite

### 🎓 Concepts to Understand

**Before starting, understand these concepts:**

1. **Test Organization**: Structure tests properly
   - Separate projects by test type (Unit, Integration, E2E)
   - Use test categories for filtering
   - Shared test utilities and fixtures

2. **Test Data Builders**: Create test data easily
   - Fluent API for building test objects
   - Reduces test code duplication
   - Makes tests more readable

3. **Code Coverage**: Measure test coverage
   - Aim for 80%+ coverage
   - Focus on business logic
   - Don't chase 100% (diminishing returns)

4. **CI/CD Integration**: Run tests in pipeline
   - Automated test execution
   - Code coverage reports
   - Test result reporting

**Key Questions to Answer:**
- How do you organize a comprehensive test suite?
- What is a good code coverage target?
- How do you integrate tests into CI/CD?

### Prompt

```text
Create a complete OrderService microservice with:

1. All features from previous POCs:
   - CRUD operations
   - External service calls
   - Event publishing
   - Resilience patterns
   - Authentication/Authorization
   - Validation
   - Background processing
2. Comprehensive test suite:
   - Unit tests (80%+ coverage)
   - Integration tests
   - End-to-end tests
3. Test infrastructure:
   - CustomWebApplicationFactory
   - Test data builders
   - Test fixtures
   - Shared test utilities
4. Test organization:
   - Separate test projects by test type
   - Use test categories
   - Use test data attributes
5. CI/CD integration:
   - Test execution in pipeline
   - Code coverage reports
   - Test result reporting

Create comprehensive test documentation:
- Test strategy document
- Test coverage report
- Test execution guide

After implementation, explain:
1. How to organize a comprehensive test suite
2. How to achieve 80%+ code coverage
3. How to integrate tests into CI/CD
4. What makes a good test strategy
```

### 📝 Progress Tracking

**Status**: ⬜ Not Started | 🟡 In Progress | ✅ Completed

**Checklist**:
- [ ] Combined all features from previous POCs
- [ ] Created comprehensive test suite
- [ ] Organized test projects by type
- [ ] Created test infrastructure (factories, builders, fixtures)
- [ ] Achieved 80%+ code coverage
- [ ] Wrote unit tests for all features
- [ ] Wrote integration tests
- [ ] Wrote end-to-end tests
- [ ] Set up CI/CD integration
- [ ] Generated code coverage reports
- [ ] Created test documentation
- [ ] All tests passing
- [ ] Understood test organization
- [ ] Understood CI/CD integration

**Notes**:
- Date Started: ___________
- Date Completed: ___________
- Key Learnings: ___________

---

## Testing Best Practices Covered

✅ **AAA Pattern** - Arrange-Act-Assert structure  
✅ **Test Isolation** - Each test is independent  
✅ **Mocking** - Use Moq for dependencies  
✅ **Test Data Builders** - Create test data easily  
✅ **In-Memory Databases** - Fast database tests  
✅ **Test Containers** - Real database testing (optional)  
✅ **Integration Testing** - Test full HTTP pipeline  
✅ **Async Testing** - Test async/await code  
✅ **Concurrency Testing** - Test race conditions  
✅ **Error Scenarios** - Test failure paths  
✅ **Code Coverage** - Measure test coverage  
✅ **Test Organization** - Structure tests properly  

---

## Tools and Libraries

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database
- **Bogus** - Test data generation
- **AutoFixture** - Test data builders
- **WireMock.NET** - HTTP mocking
- **Testcontainers** - Docker-based testing (optional)

---

## Expected Outcome

After completing all POCs, you'll have:

- Deep understanding of unit testing in .NET Core
- Proficiency with Moq for mocking
- Experience with integration testing
- Knowledge of testing microservices patterns
- Ability to write maintainable test code
- Skills to achieve high code coverage
- Understanding of testing best practices

**Time Estimate: 2-3 days for all POCs**

---

## How to Use This Guide

1. **Start with POC 1** - Learn fundamentals
2. **Follow sequentially** - Each builds on previous concepts
3. **Run tests frequently** - Verify after each test
4. **Use Cursor AI** - Copy prompts into Cursor AI chat
5. **Experiment** - Try different scenarios
6. **Review coverage** - Use code coverage tools
7. **Refactor** - Improve test code quality

---

## Quick Reference Commands

### Create Test Project
```bash
dotnet new xunit -n ServiceName.Tests
dotnet add ServiceName.Tests reference ServiceName
dotnet add ServiceName.Tests package Moq
dotnet add ServiceName.Tests package FluentAssertions
dotnet add ServiceName.Tests package Microsoft.AspNetCore.Mvc.Testing
```

### Run Tests
```bash
dotnet test
dotnet test --filter "Category=Integration"
dotnet test --logger "trx;LogFileName=test-results.trx"
```

### Code Coverage
```bash
dotnet add ServiceName.Tests package coverlet.msbuild
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

*"Good tests are a safety net that allows you to refactor with confidence."*

