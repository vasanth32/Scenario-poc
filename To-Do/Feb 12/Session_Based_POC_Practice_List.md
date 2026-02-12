# POC List to Practice (From This Session)

Below is a practice POC list with ready-to-paste prompts you can use in Calvary AI (or any coding assistant) to generate projects step by step.

---

## 1) Circular Dependency and DI Scope POC

**Goal:** Understand Parent-Child references and scoped creation.

**Prompt**

```text
Create a .NET console application demonstrating two classes (ParentService and ChildService) that reference each other.
Show:
1. Constructor-based circular dependency issue
2. Resolving it using IServiceScopeFactory and scoped resolution
3. Example execution demonstrating working resolution
Explain each step in comments.
```

---

## 2) Task.WhenAll AggregateException Handling

**Goal:** Capture all exceptions from parallel async tasks.

**Prompt**

```text
Create a .NET console app demonstrating multiple async tasks where some tasks throw exceptions.
Use Task.WhenAll to run them.
Show:
1. How await Task.WhenAll throws only one exception
2. How to capture all exceptions using AggregateException and InnerExceptions
3. Log all exceptions properly
Include sample output.
```

---

## 3) Concurrency Throttling using SemaphoreSlim

**Goal:** Limit concurrent processing to 5 tasks.

**Prompt**

```text
Create a .NET console application simulating 25 incoming messages processed asynchronously.
Use SemaphoreSlim to ensure only 5 concurrent tasks execute at any time.
Log start/end timestamps of each task to prove concurrency control.
Explain why SemaphoreSlim is better than lock for async workflows.
```

---

## 4) Strategy Pattern Discount Engine (Dynamic Rules)

**Goal:** Dynamic BuyXGetY discount.

**Prompt**

```text
Create a .NET console application implementing a discount engine using the Strategy pattern.
Implement:
1. IDiscountStrategy interface
2. PercentageDiscountStrategy
3. BuyXGetYStrategy (values loaded dynamically from configuration)
4. DiscountEngine that applies multiple strategies sequentially
Show example orders and calculated discounts.
```

---

## 5) Complex Basket Discount (Cheapest Item Free)

**Goal:** Category-based discount logic.

**Prompt**

```text
Create a .NET console project implementing a basket discount rule:
"Buy any 3 items in the same category and the cheapest item is free".
Group order items by category, sort prices, and compute total discount.
Provide sample order input and output results.
```

---

## 6) Delegate Covariance POC

**Goal:** Understand AF = DF assignment behavior.

**Prompt**

```text
Create a .NET console application demonstrating delegate covariance.
Define:
1. Animal and Dog classes
2. AnimalFactory and DogFactory delegates
3. Assign DogFactory to AnimalFactory
4. Show runtime type returned and explain why assignment works
Include comments explaining covariance.
```

---

## 7) Optimistic Concurrency (RowVersion) POC

**Goal:** Prevent multiple users overwriting data.

**Prompt**

```text
Create a .NET Web API project demonstrating optimistic concurrency using EF Core RowVersion.
Create:
1. Customer table with RowVersion column
2. Update API endpoint that checks RowVersion
3. Simulate concurrent update conflict and return appropriate error message
Explain how concurrency conflict detection works.
```

---

## 8) Dashboard Global Update (MVC + SignalR)

**Goal:** Parent dashboard notifying widgets.

**Prompt**

```text
Create an ASP.NET Core MVC project implementing a dashboard with multiple widgets.
Use:
1. Shared DashboardStateService
2. SignalR hub for real-time updates
3. Parent dashboard triggering updates
4. Widgets subscribing and refreshing automatically
Explain communication flow.
```


