# Unit Testing Concepts Guide

This guide explains the fundamental concepts of unit testing using examples from the **ProductService** implementation.

---

## 1. Unit Testing

### What is Unit Testing?

**Unit Testing** is the practice of testing individual units (methods, classes, or components) of code in isolation to ensure they work correctly.

### Key Characteristics

- **Isolation**: Each test is independent and doesn't rely on external dependencies (databases, file systems, network calls, etc.)
- **Fast**: Unit tests should run quickly (milliseconds, not seconds)
- **Repeatable**: Tests should produce the same results every time they run
- **Deterministic**: No randomness or timing dependencies

### Why Unit Testing?

1. **Catch Bugs Early**: Find issues during development, not in production
2. **Enable Refactoring**: Confidently change code knowing tests will catch regressions
3. **Document Behavior**: Tests serve as executable documentation showing how code should work
4. **Improve Design**: Writing testable code often leads to better architecture (loose coupling, dependency injection)

### Example from ProductService

```22:44:Unit Testing/ProductService.Tests/UnitTest1.cs
    [Fact]
    public async Task GetProductAsync_Returns_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            StockQuantity = 5,
            Category = "Cat"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductAsync(product.Id);

        // Assert
        result.Should().BeEquivalentTo(product);
    }
```

This test:
- ✅ Tests **only** the `ProductService.GetProductAsync` method
- ✅ Is **isolated** from the real repository (uses a mock)
- ✅ Runs **fast** (no database or network calls)
- ✅ Is **repeatable** (same result every time)

---

## 2. xUnit Framework

### What is xUnit?

**xUnit** is a popular, open-source testing framework for .NET. It's the default choice for .NET Core projects.

### Key Attributes

#### `[Fact]` Attribute
Marks a method as a test that should be executed by the test runner.

```csharp
[Fact]
public async Task GetProductAsync_Returns_Product_When_Exists()
{
    // Test code here
}
```

#### `[Theory]` Attribute
Used for **parameterized tests** - allows testing multiple scenarios with different inputs.

```csharp
[Theory]
[InlineData("", 10, 5)]  // Empty name
[InlineData("Product", 0, 5)]  // Zero price
[InlineData("Product", -1, 5)]  // Negative price
public async Task CreateProductAsync_Validates_Inputs(string name, decimal price, int stock)
{
    // Test with different parameters
}
```

### Standard Assertions (xUnit)

xUnit provides basic assertion methods:

```csharp
Assert.Equal(expected, actual);        // Values are equal
Assert.NotNull(actual);                 // Object is not null
Assert.True(condition);                 // Condition is true
Assert.Throws<Exception>(() => code);  // Code throws exception
```

**However**, we use **FluentAssertions** in our tests for better readability (see section 4).

---

## 3. Moq (Mocking)

### What is Mocking?

**Mocking** is creating fake objects (mocks) that simulate the behavior of real dependencies. This allows us to test code in isolation.

### Why Mock Dependencies?

1. **Isolation**: Test only the code under test, not its dependencies
2. **Control**: Control what dependencies return (success, failure, edge cases)
3. **Speed**: Avoid slow operations (database, network, file I/O)
4. **Reliability**: Tests don't fail due to external factors (network down, database unavailable)
5. **Edge Cases**: Easily test error scenarios (null returns, exceptions)

### Moq Basics

#### Creating a Mock

```csharp
var repositoryMock = new Mock<IProductRepository>();
```

#### Setting Up Mock Behavior (`Setup`)

Tell the mock what to return when a method is called:

```35:37:Unit Testing/ProductService.Tests/UnitTest1.cs
        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
```

This says: "When `GetByIdAsync` is called with `product.Id`, return `product`."

#### Verifying Method Calls (`Verify`)

Check that a method was called with specific parameters:

```120:126:Unit Testing/ProductService.Tests/UnitTest1.cs
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Product>(p =>
                p.Name == request.Name &&
                p.Price == request.Price &&
                p.StockQuantity == request.StockQuantity &&
                p.Category == request.Category)),
            Times.Once);
```

This verifies:
- `AddAsync` was called **exactly once** (`Times.Once`)
- It was called with a `Product` matching the request properties (`It.Is<Product>(...)`)

#### Using the Mock Object

```csharp
var sut = new ProductService(repositoryMock.Object);  // .Object gives the actual mock instance
```

### Real Example: Why We Mock `IProductRepository`

**Without Mocking** (❌ Bad):
```csharp
// This would require a real database, connection string, etc.
var repository = new ProductRepository(connectionString);
var service = new ProductService(repository);
var result = await service.GetProductAsync(1);
```

**Problems:**
- Requires database setup
- Slow (database calls)
- Tests might fail if database is unavailable
- Hard to test edge cases (what if product doesn't exist?)
- Tests are not isolated

**With Mocking** (✅ Good):
```csharp
var repositoryMock = new Mock<IProductRepository>();
repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
var service = new ProductService(repositoryMock.Object);
var result = await service.GetProductAsync(1);
```

**Benefits:**
- ✅ No database needed
- ✅ Fast execution
- ✅ Always works (no external dependencies)
- ✅ Easy to test edge cases (just change the `Setup`)
- ✅ Tests are isolated

---

## 4. FluentAssertions

### What is FluentAssertions?

**FluentAssertions** is a library that provides a fluent, readable syntax for writing assertions. It makes tests more readable and provides better error messages.

### Comparison: Standard Assertions vs FluentAssertions

#### Standard xUnit Assertions
```csharp
Assert.Equal(expected, actual);
Assert.NotNull(result);
Assert.True(result.IsValid);
Assert.Throws<ArgumentException>(() => service.CreateProduct(request));
```

#### FluentAssertions (More Readable)
```csharp
actual.Should().Be(expected);
result.Should().NotBeNull();
result.IsValid.Should().BeTrue();
await act.Should().ThrowAsync<ArgumentException>();
```

### Examples from ProductService

#### Simple Equality Check
```43:43:Unit Testing/ProductService.Tests/UnitTest1.cs
        result.Should().BeEquivalentTo(product);
```

`BeEquivalentTo` compares object properties, not reference equality. More flexible than `Assert.Equal`.

#### Exception Assertions
```60:61:Unit Testing/ProductService.Tests/UnitTest1.cs
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*42*");
```

This verifies:
- An exception of type `NotFoundException` is thrown
- The exception message contains "42"

#### Property Assertions
```128:131:Unit Testing/ProductService.Tests/UnitTest1.cs
        result.Name.Should().Be(request.Name);
        result.Price.Should().Be(request.Price);
        result.StockQuantity.Should().Be(request.StockQuantity);
        result.Category.Should().Be(request.Category);
```

### Why FluentAssertions?

1. **Readability**: `result.Should().Be(expected)` reads like English
2. **Better Error Messages**: When a test fails, FluentAssertions provides detailed information
3. **Rich API**: Many assertion methods for collections, dates, strings, etc.
4. **IntelliSense**: Better IDE support and autocomplete

---

## 5. AAA Pattern (Arrange-Act-Assert)

### What is AAA?

**AAA** is a pattern for structuring unit tests. Every test should have three distinct sections:

1. **Arrange**: Set up test data, create mocks, configure dependencies
2. **Act**: Execute the method under test
3. **Assert**: Verify the results, check exceptions, verify mock calls

### Why AAA?

- **Readability**: Clear structure makes tests easy to understand
- **Maintainability**: Easy to find where setup, execution, and verification happen
- **Consistency**: All tests follow the same pattern

### Example: AAA Pattern in Action

```22:44:Unit Testing/ProductService.Tests/UnitTest1.cs
    [Fact]
    public async Task GetProductAsync_Returns_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            StockQuantity = 5,
            Category = "Cat"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductAsync(product.Id);

        // Assert
        result.Should().BeEquivalentTo(product);
    }
```

**Breakdown:**
- **Arrange** (lines 25-37): Create test data (`product`) and configure mock behavior
- **Act** (line 40): Call the method under test (`GetProductAsync`)
- **Assert** (line 43): Verify the result matches expectations

### Another Example: Testing Validation

```64:82:Unit Testing/ProductService.Tests/UnitTest1.cs
    [Fact]
    public async Task CreateProductAsync_Validates_Name_Is_Not_Empty()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            Price = 10m,
            StockQuantity = 1,
            Category = "Cat"
        };

        // Act
        var act = async () => await _sut.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }
```

**Breakdown:**
- **Arrange** (lines 67-74): Create invalid request (empty name)
- **Act** (line 77): Wrap the call in a lambda to capture exceptions
- **Assert** (lines 80-81): Verify `ArgumentException` is thrown with message containing "name"

---

## Key Questions Answered

### 1. What is the difference between unit tests and integration tests?

| Aspect | Unit Tests | Integration Tests |
|--------|------------|-------------------|
| **Scope** | Test individual methods/classes | Test multiple components working together |
| **Dependencies** | Mocked (fake) | Real (database, APIs, file system) |
| **Speed** | Fast (milliseconds) | Slower (seconds to minutes) |
| **Isolation** | Fully isolated | May depend on external systems |
| **Purpose** | Verify logic, validation, business rules | Verify components integrate correctly |
| **Example** | Test `ProductService.CreateProductAsync` with mocked repository | Test `ProductController` with real database |

**Unit Test Example** (from our codebase):
```csharp
// Mocks repository - no real database
var mock = new Mock<IProductRepository>();
var service = new ProductService(mock.Object);
```

**Integration Test Example** (hypothetical):
```csharp
// Uses real database
var dbContext = new ApplicationDbContext(connectionString);
var repository = new ProductRepository(dbContext);
var service = new ProductService(repository);
```

### 2. Why do we mock dependencies instead of using real ones?

**Reasons:**

1. **Isolation**: Test only the code under test, not its dependencies
   - If `ProductRepository` has a bug, it shouldn't break `ProductService` tests

2. **Speed**: Real dependencies are slow
   - Database calls: 50-500ms per call
   - Network calls: 100-1000ms per call
   - Mock calls: <1ms

3. **Reliability**: Real dependencies can fail
   - Database might be down
   - Network might be unavailable
   - Tests should be deterministic

4. **Control**: Easy to test edge cases
   ```csharp
   // Test what happens when repository returns null
   mock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product?)null);
   
   // Test what happens when repository throws exception
   mock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception("DB Error"));
   ```

5. **Simplicity**: No need to set up databases, configure connections, seed data

**Real Example from ProductService:**
```13:20:Unit Testing/ProductService.Tests/UnitTest1.cs
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService.Services.ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new ProductService.Services.ProductService(_repositoryMock.Object);
    }
```

We mock `IProductRepository` so we can:
- Test `ProductService` logic without a real database
- Control what the repository returns
- Test error scenarios easily
- Run tests fast

### 3. How does the AAA pattern make tests more readable?

**Without AAA** (❌ Hard to Read):
```csharp
[Fact]
public async Task Test1()
{
    var product = new Product { Id = 1, Name = "Test", Price = 10m, StockQuantity = 5, Category = "Cat" };
    _repositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
    var result = await _sut.GetProductAsync(product.Id);
    result.Should().BeEquivalentTo(product);
}
```

**With AAA** (✅ Clear Structure):
```22:44:Unit Testing/ProductService.Tests/UnitTest1.cs
    [Fact]
    public async Task GetProductAsync_Returns_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test",
            Price = 10m,
            StockQuantity = 5,
            Category = "Cat"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductAsync(product.Id);

        // Assert
        result.Should().BeEquivalentTo(product);
    }
```

**Benefits:**
- ✅ **Clear Intent**: Comments make it obvious what each section does
- ✅ **Easy to Scan**: Quickly find where setup, execution, and verification happen
- ✅ **Consistent**: All tests follow the same pattern
- ✅ **Maintainable**: Easy to modify - know exactly where to add new setup or assertions

**When a test fails:**
- You immediately know if it's a setup issue (Arrange)
- An execution issue (Act)
- Or a verification issue (Assert)

---

## Summary

| Concept | Purpose | Key Takeaway |
|---------|---------|--------------|
| **Unit Testing** | Test individual units in isolation | Fast, repeatable, isolated tests catch bugs early |
| **xUnit** | Testing framework for .NET | Use `[Fact]` for tests, `[Theory]` for parameterized tests |
| **Moq** | Create fake dependencies | Mock dependencies to isolate code under test and control behavior |
| **FluentAssertions** | Readable assertions | `result.Should().Be(expected)` is more readable than `Assert.Equal` |
| **AAA Pattern** | Structure tests | Arrange → Act → Assert makes tests clear and maintainable |

---

## Next Steps

1. Review the test file: `ProductService.Tests/UnitTest1.cs`
2. Run the tests: `dotnet test`
3. Try adding your own tests following the AAA pattern
4. Experiment with different Moq setups and verifications
5. Explore more FluentAssertions methods

---

**Remember**: Good unit tests are fast, isolated, repeatable, and easy to read. The AAA pattern, mocking, and FluentAssertions help achieve these goals!

