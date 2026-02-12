# Async / Task / ValueTask POC Practice List (Beginner -> Advanced)

Below is a POC practice list with ready-to-use prompts you can paste into Calvary AI.

These are arranged from beginner to advanced.

---

## POC 1 - Basic async Task method

**Goal:** Understand Task creation and awaiting.

**Prompt to use:**

```text
Create a simple .NET console application demonstrating an async method returning Task.
The method should simulate a database call using Task.Delay and return a string.
Show how the caller awaits the method and prints the result.
Explain each step clearly.
```

---

## POC 2 - Multiple Tasks running in parallel

**Goal:** Understand multiple Task instances.

**Prompt to use:**

```text
Create a .NET console example showing three async methods running in parallel using Task.WhenAll.
Each method should simulate a delay and return a value.
Print when each task starts and completes to show parallel execution.
Explain the flow.
```

---

## POC 3 - Exception propagation in async Task

**Goal:** Understand how exceptions flow.

**Prompt to use:**

```text
Create a .NET console program demonstrating exception handling in async Task methods.
One async method should throw an exception.
Show how the caller catches the exception using try-catch with await.
Explain why exception is properly propagated.
```

---

## POC 4 - async void exception problem (Important)

**Goal:** See the real problem with async void.

**Prompt to use:**

```text
Create a .NET example comparing async void and async Task methods.
Both methods should throw an exception.
Demonstrate how async Task exception can be caught but async void exception cannot be caught by the caller.
Explain the behavior clearly.
```

---

## POC 5 - Task vs ValueTask performance scenario

**Goal:** Learn when ValueTask helps.

**Prompt to use:**

```text
Create a .NET example demonstrating Task vs ValueTask usage.
Simulate a cache lookup where data is sometimes available immediately and sometimes fetched asynchronously.
Return ValueTask when data is cached and Task when fetching from database.
Explain when ValueTask avoids allocation.
```

---

## POC 6 - Fire-and-forget background work (safe pattern)

**Goal:** Proper alternative to async void.

**Prompt to use:**

```text
Create a .NET example demonstrating a safe fire-and-forget background task pattern using Task.Run instead of async void.
Include proper exception logging inside the background task.
Explain why this approach is safer than async void.
```

---

## Very Important (Your Level - 7 Years Experience)

If you practice these 6 POCs, you will be able to confidently answer:

- Task lifecycle
- Parallel tasks
- Exception propagation
- async void dangers
- ValueTask real-time use
- Fire-and-forget patterns

These questions are extremely common in senior .NET interviews.


