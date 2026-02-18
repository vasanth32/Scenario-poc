## ProductService Unit Testing POC Progress

We implemented a separate test project `ProductService.Tests` using xUnit, Moq, and FluentAssertions, and wrote unit tests for `ProductService` that follow the Arrange-Act-Assert (AAA) pattern.

1. **Why we mock `IProductRepository` instead of using the real implementation**
   - We want to unit test only the business logic inside `ProductService`, not the behavior of the in-memory list (or any future persistence). By mocking `IProductRepository`, we can precisely control inputs/outputs (e.g., returning a specific `Product` or `null`) and verify interactions without worrying about data access details or side effects.

2. **How the AAA pattern is applied in each test**
   - Each test is structured into three clear sections:
     - **Arrange**: set up the mock repository behavior and create request objects or domain entities.
     - **Act**: call the specific method on `ProductService` under test.
     - **Assert**: use FluentAssertions (and Moq verifications) to check the returned results, thrown exceptions, and that the expected repository methods were invoked.

3. **What FluentAssertions provides over standard `Assert` methods**
   - FluentAssertions offers a more readable, fluent syntax (e.g., `result.Should().BeEquivalentTo(expected)` or `act.Should().ThrowAsync<NotFoundException>()`) and rich, informative failure messages. This makes test intent clearer and simplifies complex assertions (like comparing entire objects) compared to multiple `Assert.Equal`/`Assert.True` statements.

4. **How to verify that a mocked method was called with specific parameters**
   - Moq allows us to verify calls with argument matching. For example:
     - ` _repositoryMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == request.Name && p.Price == request.Price)), Times.Once);`
   - This ensures that `AddAsync` was called exactly once with a `Product` whose properties match the expected values, giving us confidence that `ProductService` correctly maps request data to domain objects and interacts with the repository as intended.


## Integration Testing Concepts (for next POC)

Before writing integration tests for `ProductService`, these are the core ideas and how to think about them in this project:

1. **Integration Testing vs Unit Testing**
   - **Unit tests** (what we already did) focus on a single class or method in isolation, using mocks to fake dependencies.
   - **Integration tests** exercise **multiple pieces together**: the full HTTP pipeline (routing, middleware, controllers, DI, services, and even the database).
   - In our context, a unit test calls `ProductService` directly; an integration test will send a real HTTP request (e.g., `GET /api/product/1`) and verify the **actual HTTP response** that comes back.

2. **`WebApplicationFactory<TProgram>` – test server for your API**
   - `WebApplicationFactory<Program>` can **host the ProductService API in-memory** for tests, without you running `dotnet run` manually.
   - It gives you an `HttpClient` that behaves like a real client calling your API over HTTP, so things like routing, model binding, filters, and middleware are all involved.
   - You can also **override configuration** inside the factory (for example, to swap the real database with an in-memory database just for tests).

3. **In-Memory Database for integration tests**
   - For integration tests that hit the database, you usually don't want to use your real SQL Server instance.
   - Instead, you configure EF Core with `UseInMemoryDatabase("TestDbName")` in the test environment:
     - It is **fast** and easy to set up.
     - It avoids side effects on your real data.
     - It is “good enough” for most integration test scenarios, especially for learning and POCs.
   - In this ProductService POC, if we introduce EF Core later, we’ll plug in an in-memory provider inside the test factory rather than pointing at a real database.

4. **Test Isolation**
   - Each integration test should **not depend** on data left behind by another test.
   - Common strategies:
     - Use a **unique in-memory database name per test** (or per test class), so each test sees a clean database.
     - Or, **clean/seed** the database at the start of each test (e.g., delete all rows, then insert known test data).
   - The goal is that you can run tests:
     - In any order,
     - Repeatedly,
     - On any machine,
     and they always produce the same results.

5. **Why use `WebApplicationFactory` instead of starting the app manually?**
   - You do **not** have to:
     - Run `dotnet run`,
     - Find the port,
     - And then call the API with an external client.
   - `WebApplicationFactory` spins up the app **inside the test process**:
     - Tests stay fast and deterministic.
     - You can easily swap dependencies (e.g., inject in-memory DB, fake external services).
     - Setup/teardown is automatic and controlled in code, instead of being a manual step outside the test runner.

6. **When to use in-memory database vs real database**
   - **In-memory database**:
     - Great for **most integration tests** in CI and for learning.
     - Fast, easy to reset, no external dependencies.
   - **Real database** (local SQL Server, container, or test DB):
     - Useful when you want to test **real DB-specific behavior** (transactions, concurrency, specific SQL features).
     - Slower and requires more setup/maintenance.
   - For this ProductService POC, starting with **in-memory database** (when we add EF Core) is usually the best trade-off: you get realistic HTTP + DI + persistence behavior without the overhead of a real database.


## Integration Testing Implementation Notes

1. **How `WebApplicationFactory` creates a test server**
   - We created `CustomWebApplicationFactory : WebApplicationFactory<Program>` in the `ProductService.IntegrationTests` project.
   - When the tests run, xUnit uses this factory to **host the real ProductService application in-memory**.
   - The call to `factory.CreateClient()` returns an `HttpClient` that sends HTTP requests through the full ASP.NET Core pipeline (middleware, routing, model binding, controllers, filters, etc.) without needing to run `dotnet run` manually.

2. **Why we override `ConfigureWebHost` to use an in-memory database**
   - In `CustomWebApplicationFactory.ConfigureWebHost` we:
     - Remove the normal `ProductDbContext` registration that points to SQL Server.
     - Re-register `ProductDbContext` using `UseInMemoryDatabase("ProductServiceTestDb")`.
     - Ensure the database is created and seed sample products.
   - This means integration tests:
     - Do **not** talk to your real `VASANTH\\SQLEXPRESS` SQL Server.
     - Run quickly and safely, with a fresh, in-memory database for testing.

3. **How integration tests differ from unit tests in this solution**
   - **Unit tests (`ProductService.Tests`)**
     - Call `ProductService` methods directly.
     - Mock `IProductRepository` with Moq.
     - Do **not** go through controllers, routing, or the real database.
   - **Integration tests (`ProductService.IntegrationTests`)**
     - Use `HttpClient` from `CustomWebApplicationFactory` to call endpoints like `GET /api/products` and `POST /api/products`.
     - Exercise the full stack: middleware → routing → controller → `ProductService` → EF Core repository → in-memory database.
     - Verify both the HTTP responses **and** the database state.

4. **What parts of the application are tested**
   - **In unit tests**
     - Focus: `ProductService` business rules (validation, thrown exceptions, repository interactions).
     - Dependencies (`IProductRepository`) are mocked, so no real persistence or HTTP behavior is tested.
   - **In integration tests**
     - Focus: end-to-end behavior of API endpoints.
     - Covered pieces:
       - Routing and attribute routes (e.g., `api/products`).
       - Model binding from JSON bodies to request models.
       - Controller actions and HTTP status codes (200/201/204/etc.).
       - DI wiring for `IProductService`, `IProductRepository`, and `ProductDbContext`.
       - EF Core repository logic and actual reads/writes to the in-memory database.


## Repository Testing Concepts (for repository-focused POCs)

1. **Repository Pattern in this solution**
   - The `IProductRepository` + `ProductRepository` pair is our implementation of the **Repository Pattern**.
   - `ProductService` only depends on the **abstraction** (`IProductRepository`), so it doesn’t care whether data comes from:
     - An in-memory list,
     - SQL Server via EF Core,
     - Or an in-memory database used just for tests.
   - This separation:
     - Keeps business logic (`ProductService`) clean.
     - Makes it easy to **swap implementations**.
     - Makes both unit testing (with mocks) and repository testing (with a real DbContext) straightforward.

2. **Why test repositories separately from services**
   - Service tests (what we already have) make sure **business rules** are correct (validation, exception handling, calling the right repository methods).
   - Repository tests focus on **data access correctness**:
     - Inserts/updates/deletes are persisted as expected.
     - Queries return the right entities/filters.
   - Testing repositories directly helps catch issues like:
     - Wrong `Include`/`Where` clauses.
     - Incorrect key mappings.
     - Missing `AsNoTracking` where appropriate.

3. **Using in-memory database for repository tests**
   - For repository tests, we can create a `DbContextOptions<ProductDbContext>` using `UseInMemoryDatabase`:
     - Very fast, fully in-memory.
     - No need for SQL Server or a connection string.
   - This lets us test `ProductRepository` end-to-end (through EF Core) without external dependencies, similar to how integration tests avoid the real SQL Server.

4. **`DbContextOptionsBuilder` and per-test DbContext**
   - Typical pattern for repository tests:
     - Use `new DbContextOptionsBuilder<ProductDbContext>()`
       - For example: `options.UseInMemoryDatabase(Guid.NewGuid().ToString());`
     - Create a **new `ProductDbContext` instance per test** with those options.
   - Benefits:
     - Each test gets a **clean database** (no leftover rows from previous tests).
     - You can easily seed data inside the test (Arrange step).

5. **Test isolation for repository tests**
   - Just like with integration tests, repository tests must not interfere with each other.
   - Using a **unique in-memory database name per test** (e.g., `Guid.NewGuid().ToString()`) guarantees that:
     - Data written in one test cannot affect another.
     - Tests are deterministic and order-independent.

6. **How in-memory database helps vs SQL Server for repo tests**
   - In-memory DB:
     - Faster to create/destroy than a real database.
     - Easy to reset between tests.
     - No need for SQL Server setup, logins, or migrations.
   - SQL Server:
     - Better when you want to verify **database-specific behavior** (constraints, transactions, performance).
   - For learning and typical repository unit/integration tests, starting with **in-memory** is usually the right balance between realism and speed.

