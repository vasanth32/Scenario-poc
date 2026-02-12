# DI Lifetime Practice - POC List + Prompts

Great idea. Practicing this with small POCs will make DI lifetimes permanently clear.

Below are POC practice tasks with ready-to-paste prompts you can give to Calvary AI (or any coding AI).

---

## POC 1 - Demonstrate Singleton, Scoped, Transient behavior

**Goal:** Show how many instances are created.

**Prompt to use:**

```text
Create an ASP.NET Core Web API project demonstrating dependency injection lifetimes.

1. Create three services:
   - SingletonService
   - ScopedService
   - TransientService

2. Each service should generate a GUID in constructor.

3. Create a controller endpoint /lifetimes that injects all three services and returns their GUID values.

4. Call the endpoint multiple times and show:
   - Singleton remains same
   - Scoped changes per request
   - Transient changes per resolution

Use .NET 8 minimal setup.
```

---

## POC 2 - Show runtime error (Scoped injected into Singleton)

**Goal:** Understand why framework blocks it.

**Prompt to use:**

```text
Create an ASP.NET Core Web API project where:

1. Register a Scoped service named UserContext.
2. Register a Singleton service named ReportManager.
3. Inject UserContext directly into ReportManager constructor.

Run the application and show the runtime exception explaining why scoped service cannot be consumed from singleton.

Add comments explaining the lifecycle mismatch.
```

---

## POC 3 - Correct pattern: Singleton using CreateScope()

**Goal:** Learn proper fix.

**Prompt to use:**

```text
Create an ASP.NET Core Web API project demonstrating how a Singleton service correctly uses a Scoped service.

1. Create Scoped service AppDbContextMock.
2. Create Singleton service CloudTransferService.
3. Inject IServiceProvider into CloudTransferService.
4. Inside a method, create a scope using CreateScope(), resolve AppDbContextMock, log its GUID, and dispose scope.

Create an endpoint /transfer that calls the singleton method.

Explain in comments why this approach works safely.
```

---

## POC 4 - Show multiple scopes creating multiple scoped instances

**Goal:** Understand scope-based instance creation.

**Prompt to use:**

```text
Extend the previous project:

Inside CloudTransferService method:
1. Create two different scopes.
2. Resolve AppDbContextMock from each scope.
3. Return both GUID values in response.

Demonstrate that both GUIDs are different, proving scoped instances are per-scope.
```

---

## POC 5 - Production scenario: BackgroundService using DbContext

**Goal:** Real enterprise usage.

**Prompt to use:**

```text
Create a Worker Service (.NET BackgroundService) example:

1. Register DbContextMock as Scoped.
2. Create BackgroundWorker class (Singleton).
3. Inject IServiceProvider.
4. Inside ExecuteAsync loop, create scope, resolve DbContextMock, log GUID, delay 5 seconds.
5. Dispose scope properly.

Add comments explaining why CreateScope() is required in background services.
```

---

## Extremely Important (For Your Career Growth)

These 5 POCs together cover most DI lifetime interview questions asked for 5-10 years experienced .NET developers.


