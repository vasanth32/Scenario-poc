# Clean Architecture - Project References Guide

## Overview

Clean Architecture enforces strict dependency rules to maintain separation of concerns and ensure that business logic remains independent of external frameworks and infrastructure. This document explains how project references should be structured in a Clean Architecture solution.

## The Dependency Rule (Fundamental Principle)

**Source code dependencies can only point inward.** 

- Inner layers (Domain) must not depend on outer layers (Infrastructure, API)
- Outer layers can depend on inner layers
- This ensures that business logic remains independent and testable

## Layer Structure and Reference Rules

```
┌─────────────────────────────────────┐
│   API Layer (Presentation)         │  ← Outermost Layer
│   - Controllers                     │
│   - HTTP endpoints                  │
│   - DTOs/Mapping                    │
└─────────────────────────────────────┘
           ↓ depends on
┌─────────────────────────────────────┐
│   Infrastructure Layer              │
│   - Data Access (EF Core)           │
│   - External Services               │
│   - Repository Implementations      │
└─────────────────────────────────────┘
           ↓ depends on
┌─────────────────────────────────────┐
│   Application Layer                 │
│   - Use Cases                       │
│   - Interfaces (Contracts)          │
│   - Application Services            │
└─────────────────────────────────────┘
           ↓ depends on
┌─────────────────────────────────────┐
│   Domain Layer (Core)               │  ← Innermost Layer
│   - Entities                        │
│   - Value Objects                   │
│   - Domain Services                 │
│   - Business Logic                  │
└─────────────────────────────────────┘
```

## Detailed Reference Rules by Layer

### 1. Domain Layer (Innermost)

**Purpose:** Contains core business logic and entities

**Allowed References:**
- ✅ **NONE** - The Domain layer should have **zero project references**
- ✅ Only .NET standard library or pure C# libraries (if absolutely necessary)
- ✅ No external frameworks (EF Core, ASP.NET, etc.)

**What it Contains:**
- Entities (e.g., `Order`, `Product`)
- Value Objects
- Domain Services
- Domain Events
- Business Rules and Validations

**Example from this project:**
```xml
<!-- CleanArch.Orders.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <!-- NO ProjectReference or PackageReference -->
</Project>
```

**Why:** The Domain layer is the heart of your application. It must remain pure and independent so that:
- Business logic can be tested without any external dependencies
- The domain can be reused across different applications
- Changes to infrastructure don't affect business rules

---

### 2. Application Layer

**Purpose:** Contains use cases, application services, and defines interfaces for infrastructure

**Allowed References:**
- ✅ **Domain Layer** (required)
- ✅ Application-specific packages (e.g., MediatR, AutoMapper)
- ❌ **NOT** Infrastructure Layer
- ❌ **NOT** API Layer
- ❌ **NOT** EF Core or other data access frameworks

**What it Contains:**
- Use Cases / Application Services
- Interfaces (e.g., `IOrderRepository`)
- DTOs (Data Transfer Objects)
- Application-specific exceptions
- Command/Query handlers (if using CQRS)

**Example from this project:**
```xml
<!-- CleanArch.Orders.Application.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\CleanArch.Orders.Domain\CleanArch.Orders.Domain.csproj" />
  </ItemGroup>
  <!-- Only Domain reference -->
</Project>
```

**Key Principle:** The Application layer defines **WHAT** needs to be done (interfaces), not **HOW** it's done (implementations).

**Why:** 
- Application logic remains independent of data access technology
- You can swap EF Core for Dapper, MongoDB, or any other data store without changing application code
- Interfaces defined here are implemented in the Infrastructure layer

---

### 3. Infrastructure Layer

**Purpose:** Implements data access, external services, and framework-specific concerns

**Allowed References:**
- ✅ **Domain Layer** (to use entities)
- ✅ **Application Layer** (to implement interfaces)
- ✅ External packages (EF Core, HttpClient, etc.)
- ❌ **NOT** API Layer

**What it Contains:**
- Repository Implementations (e.g., `OrderRepository : IOrderRepository`)
- DbContext (EF Core)
- External API clients
- File system access
- Email services
- Caching implementations

**Example from this project:**
```xml
<!-- CleanArch.Orders.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\CleanArch.Orders.Domain\CleanArch.Orders.Domain.csproj" />
    <ProjectReference Include="..\CleanArch.Orders.Application\CleanArch.Orders.Application.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**Key Principle:** Infrastructure implements the contracts (interfaces) defined in the Application layer.

**Why:**
- All framework-specific code is isolated here
- Changes to data access technology only affect this layer
- Application layer remains testable with mock implementations

---

### 4. API Layer (Presentation)

**Purpose:** Handles HTTP requests, routing, and presentation concerns

**Allowed References:**
- ✅ **Application Layer** (to use services and interfaces)
- ✅ **Infrastructure Layer** (for dependency injection setup)
- ✅ ASP.NET Core packages
- ❌ **NOT** Domain Layer (access through Application layer only)

**What it Contains:**
- Controllers
- Middleware
- Dependency Injection configuration
- Swagger/OpenAPI setup
- Request/Response DTOs (if not in Application layer)

**Example from this project:**
```xml
<!-- CleanArch.Orders.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <ProjectReference Include="..\CleanArch.Orders.Application\CleanArch.Orders.Application.csproj" />
    <ProjectReference Include="..\CleanArch.Orders.Infrastructure\CleanArch.Orders.Infrastructure.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.23" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>
</Project>
```

**Key Principle:** Controllers should only call Application services, never directly access repositories or entities.

**Why:**
- Presentation logic is separated from business logic
- API can be replaced with gRPC, GraphQL, or console app without changing business logic
- Easy to add multiple presentation layers (Web API, MVC, Blazor, etc.)

---

## Reference Flow Diagram

```
API Layer
  ├─→ Application Layer (uses services, interfaces)
  └─→ Infrastructure Layer (for DI registration)

Infrastructure Layer
  ├─→ Application Layer (implements interfaces)
  └─→ Domain Layer (uses entities)

Application Layer
  └─→ Domain Layer (uses entities, business logic)

Domain Layer
  └─→ (nothing - pure business logic)
```

## Dependency Injection Setup

The API layer is responsible for wiring up dependencies. In `Program.cs`:

```csharp
// Register Infrastructure implementations
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<OrderDbContext>(options => 
    options.UseInMemoryDatabase("OrdersDb"));

// Register Application services
builder.Services.AddScoped<OrderService>();
```

**Important:** The API layer knows about both Application and Infrastructure to wire them together, but business logic flows through Application layer only.

## Common Mistakes to Avoid

### ❌ Mistake 1: Domain references Infrastructure
```xml
<!-- WRONG -->
<ProjectReference Include="..\CleanArch.Orders.Infrastructure\..." />
```
**Problem:** Domain becomes coupled to data access technology

### ❌ Mistake 2: Application references Infrastructure
```xml
<!-- WRONG -->
<ProjectReference Include="..\CleanArch.Orders.Infrastructure\..." />
```
**Problem:** Application layer becomes coupled to implementation details

### ❌ Mistake 3: API directly uses Domain entities
```csharp
// WRONG - Controller directly using Domain entity
public IActionResult GetOrder(int id)
{
    var order = _repository.GetById(id); // Returns Domain.Order
    return Ok(order); // Exposing domain entity
}
```
**Solution:** Use DTOs from Application layer

### ❌ Mistake 4: Infrastructure defines interfaces
```csharp
// WRONG - Interface in Infrastructure layer
public interface IOrderRepository { }
```
**Solution:** Interfaces belong in Application layer

### ❌ Mistake 5: Domain uses EF Core attributes
```csharp
// WRONG - Domain entity with EF Core attributes
public class Order
{
    [Key] // EF Core attribute in Domain!
    public int Id { get; set; }
}
```
**Solution:** Use Fluent API in Infrastructure's DbContext

## Best Practices

### ✅ DO:
1. **Keep Domain pure** - No external dependencies
2. **Define interfaces in Application** - Not in Infrastructure
3. **Use DTOs** - Don't expose domain entities directly
4. **Dependency Injection** - Wire up in API/Infrastructure layer
5. **Test Domain independently** - No mocks needed for pure business logic

### ❌ DON'T:
1. **Don't reference outer layers from inner layers**
2. **Don't put business logic in controllers**
3. **Don't use framework attributes in Domain entities**
4. **Don't create circular dependencies**
5. **Don't skip the Application layer** - Even for simple CRUD, use it

## Verification Checklist

Use this checklist to verify your Clean Architecture setup:

- [ ] Domain layer has **zero** project references
- [ ] Application layer references **only** Domain
- [ ] Infrastructure layer references Domain and Application
- [ ] API layer references Application and Infrastructure
- [ ] All interfaces are defined in Application layer
- [ ] All implementations are in Infrastructure layer
- [ ] Controllers only call Application services
- [ ] Domain entities have no framework attributes
- [ ] Dependency injection is configured in API layer
- [ ] Domain layer can be tested without any mocks

## Summary

The reference rules in Clean Architecture create a **one-way dependency flow**:

```
API → Application → Domain
Infrastructure → Application → Domain
```

This ensures:
- **Testability**: Each layer can be tested independently
- **Maintainability**: Changes in one layer don't cascade
- **Flexibility**: Easy to swap implementations (e.g., EF Core → Dapper)
- **Independence**: Business logic is framework-agnostic

Remember: **Dependencies point inward, never outward!**

