## Deadlock POC Plan (Max 20 Minutes)

**Goal**: Quickly understand what a deadlock is, how it appears in a microservice, and how to fix/avoid it, using this solution (`ProductService`, `OrderService`, `PaymentService`) with minimal code changes.

**Time budget**: ~20 minutes hands‑on (not counting build/restore time).

---

## 1. Concept to Demonstrate

- **Type of deadlock**: Application‑level deadlock between two concurrent requests inside **OrderService**, using `lock`/`SemaphoreSlim` to simulate competing access to shared resources.
- **What you’ll see**:
  - Requests that **never complete** (hang) or time out.
  - Elevated **response times** in logs / `X-Response-Time` header.
  - Thread starvation under load.
- **What you’ll practice**:
  - Creating a **minimal deadlock scenario** on purpose.
  - **Observing** it with the existing logging + response‑time middleware + (optionally) Application Insights.
  - Applying **ordering + timeouts + retries** to remove the deadlock.

---

## 2. Quick Setup (2–3 minutes)

1. **Run services** (already done earlier, repeat if needed):
   - `ProductService` on `http://localhost:5001`
   - `OrderService` on `http://localhost:5002`
   - `PaymentService` on `http://localhost:5003`
2. Ensure **Redis** is running (for ProductService) – not strictly required for deadlock POC but fine to keep on.

---

## 3. Create an Intentional Deadlock in OrderService (8–10 minutes)

### 3.1 Design

- Add **two in‑memory locks** representing two “resources”:
  - `lockA` → e.g. “Inventory service”
  - `lockB` → e.g. “Payment gateway”
- Add **two endpoints** in `OrdersController`:
  - `POST /api/orders/deadlock/a` – acquires `lockA` then, after small delay, tries `lockB`.
  - `POST /api/orders/deadlock/b` – acquires `lockB` then, after small delay, tries `lockA`.
- When both endpoints are hit **concurrently**, each thread holds one lock and waits forever for the other → **classic deadlock**.

### 3.2 Implementation Tasks

> You can ask the agent to generate the exact code when you’re ready; this list is the high‑level plan.

- In `OrderService`:
  1. Add a static class (e.g. `DeadlockDemoLocks`) or static fields in `OrdersController`:
     - Two `object` or `SemaphoreSlim` instances: `ResourceALock`, `ResourceBLock`.
  2. Add two new action methods in `OrdersController`:
     - `DeadlockScenarioA()`:
       - `lock(ResourceALock)` → `Task.Delay(1000)` → `lock(ResourceBLock)` → return simple JSON.
     - `DeadlockScenarioB()`:
       - `lock(ResourceBLock)` → `Task.Delay(1000)` → `lock(ResourceALock)` → return simple JSON.
  3. Ensure these endpoints **do not touch the database** to keep the example focused and fast.

---

## 4. Trigger and Observe the Deadlock (5 minutes)

1. **Build & run** `OrderService`:
   - `dotnet run --urls "http://localhost:5002"`
2. From two terminals (or Postman):
   - Terminal 1:
     ```bash
     curl -X POST http://localhost:5002/api/orders/deadlock/a
     ```
   - Terminal 2 (start within ~1s of the first):
     ```bash
     curl -X POST http://localhost:5002/api/orders/deadlock/b
     ```
3. **Expected behavior**:
   - Both requests **hang** (no response) unless you cancel them.
   - `OrderService` logs show requests starting but **no completion** lines.
   - CPU may stay low but threads are blocked.

4. (Optional, if AI connection string is configured):
   - In Application Insights **Live Metrics** or **Logs**, filter by `cloud_RoleName == "OrderService"` and watch:
     - Requests stuck in “in progress”.
     - Custom metric `RequestDurationMs` climbing for those calls.

---

## 5. Fix the Deadlock (5–7 minutes)

### 5.1 Strategy: Consistent Lock Ordering + Timeouts

- **Rule**: Every place that locks multiple resources must **always acquire them in the same order**.
  - E.g. always lock `A` then `B` (never `B` then `A`).
- **Timeouts**: Use `SemaphoreSlim.WaitAsync(timeout)` / `Monitor.TryEnter` to **avoid waiting forever**, and return 409/503 instead.

### 5.2 Refactor Plan

- Replace `lock` with `SemaphoreSlim` for each resource:
  - `SemaphoreSlim ResourceALock = new(1, 1);`
  - `SemaphoreSlim ResourceBLock = new(1, 1);`
- Update both endpoints to:
  - Always `await ResourceALock.WaitAsync(...)` then `ResourceBLock.WaitAsync(...)` (same order).
  - Use `try/finally` to `Release()` both semaphores.
  - If lock acquisition times out:
    - Log a warning: “Deadlock avoided: could not acquire ResourceBLock within X ms”.
    - Return `StatusCode(409, "Could not acquire lock, please retry")`.
- Optionally track custom metrics:
  - `OrdersLockTimeouts` metric when a timeout occurs.

---

## 6. Verify the Fix (3–5 minutes)

1. Rebuild & restart `OrderService`.
2. Run the same two concurrent `curl` commands:
   - Requests should now **complete**:
     - One succeeds quickly; the other may be delayed or receive 409/503 instead of hanging.
3. Check:
   - Logs: no unbounded hangs; see warnings only when timeouts occur.
   - Response headers: `X-Response-Time` for both calls shows finite durations.
   - (Optional) Application Insights metrics:
     - `RequestDurationMs` should no longer show unbounded growth.
     - `OrdersQueueDepth` + any custom lock timeout metric help visualize contention.

---

## 7. How This Maps to Real‑Time / Production

- **DB deadlocks**: Instead of `lock` objects, the “resources” are **rows/tables**; the fix is the same:
  - Consistent access order (e.g., always update `Orders` before `Payments`).
  - Short transactions + **retry policies** (e.g. Polly or EF Core execution strategy).
- **Distributed deadlocks between microservices**:
  - Circular call chains (Service A → B → C → A) with blocking calls.
  - Break cycles via **timeouts**, **circuit breakers**, and **asynchronous messaging** (queues/events).
- This POC gives you a **mental model** and a **reproducible lab** inside this solution so you can extend it later to:
  - DB‑level deadlock demos.
  - Saga / outbox pattern for cross‑service workflows.

---

## 8. Next Steps (Optional, After the 20 Minutes)

- Add a **database‑backed** deadlock example with two transactions updating tables in opposite order.
- Emit **custom events** to Application Insights when a potential deadlock is detected or a retry is triggered.
- Create a **Dashboard** in Azure Portal visualizing:
  - `RequestDurationMs` (P95, P99)
  - `OrdersQueueDepth`
  - `CacheHitRate`
  - Lock timeout / deadlock‑avoidance counts.


