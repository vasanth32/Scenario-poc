## DeadlockDemo – Progress & Learning Notes

### Step 1 – Project & Database Setup

- Created a **.NET 8 Web API** project called `DeadlockDemo`.
- Added **EF Core SQL Server** packages and configured:
  - `DeadlockDemoDbContext` with two tables:
    - `Orders`: `OrderId` (PK), `ProductId`, `Quantity`, `Status`.
    - `Inventory`: `ProductId` (PK), `AvailableQty`, `ReservedQty`.
- Configured the connection string in `appsettings.json` to point to your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=VASANTH\\SQLEXPRESS;Database=DeadlockDemoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

**Why this matters:** this mirrors a real production app: one API, one DB, and clear table design for orders + stock.

---

### Step 2 – Migrations & Seeding

- Added EF Core migrations (`InitialCreate`) and wired up a **seeder**:
  - `DeadlockDemoSeeder.Seed(app)` is called on startup.
  - It runs `context.Database.Migrate()` to apply migrations automatically.
  - If the tables are empty, it inserts:
    - `Orders`: `OrderId = 1`, `ProductId = 101`, `Quantity = 2`, `Status = "Placed"`.
    - `Inventory`: `ProductId = 101`, `AvailableQty = 100`, `ReservedQty = 0`.

**Why this matters:** you get a **known, repeatable starting state** for the deadlock demo without manually inserting rows.

---

### Step 3 – Deadlock POC API 1: `/api/orders/place`

- Registered **MVC controllers** in `Program.cs`:
  - `builder.Services.AddControllers();`
  - `app.MapControllers();`
- Added `OrdersController` with `POST /api/orders/place`.
- Behavior of `PlaceOrder`:
  1. Loads the seed `Order` (Id = 1) and its `Inventory` record.
  2. Starts a **database transaction**:
     - **Step 1 – Inventory first**:
       - `inventory.ReservedQty += order.Quantity;`
       - `SaveChangesAsync()`.
     - **Simulated delay**:
       - `await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);`
       - This keeps locks on both rows for 5 seconds.
     - **Step 2 – Orders second**:
       - `order.Status = "PlacedAgain";`
       - `SaveChangesAsync()`.
     - Commits the transaction on success.
  3. On error, logs the exception, rolls back the transaction, and returns HTTP 500.

**Why this matters for deadlocks:**

- This endpoint **always touches Inventory first, then Orders** inside a single transaction and holds locks for 5 seconds.
- When we later create a second endpoint that updates **Orders first, then Inventory** (with the same delay), running them concurrently will give SQL Server the classic deadlock pattern:
  - Transaction A: locks Inventory row → waits on Orders row.
  - Transaction B: locks Orders row → waits on Inventory row.
  - SQL Server detects the deadlock and kills one transaction (error 1205).

In other words, `/api/orders/place` is half of the deadlock puzzle: it defines **one ordering of locks** (Inventory → Orders). The second API (CancelOrder) will intentionally use the **opposite order** so you can see deadlocks and then later fix them with retries and consistent ordering.

---

### Step 4 – Deadlock POC API 2: `/api/orders/cancel`

- Added a second endpoint in `OrdersController`:

```csharp
[HttpPost("cancel")]
public async Task<IActionResult> CancelOrder(CancellationToken cancellationToken)
{
    // Step 1: Orders first
    order.Status = "Cancelled";
    await _context.SaveChangesAsync(cancellationToken);

    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

    // Step 2: Inventory second
    inventory.ReservedQty -= order.Quantity;
    await _context.SaveChangesAsync(cancellationToken);
}
```

**What’s different from `place`:**

- `place` transaction: **Inventory → delay → Orders**.
- `cancel` transaction: **Orders → delay → Inventory**.
- Both use the same seed order and inventory row, and both run for ~5 seconds while holding locks.

**Why this creates a real SQL deadlock:**

- Suppose the two endpoints run at nearly the same time:
  - Transaction A (`place`): locks Inventory row first, then waits on Orders row.
  - Transaction B (`cancel`): locks Orders row first, then waits on Inventory row.
- Each transaction now **holds one lock and needs the other** → circular wait.
- SQL Server’s deadlock detector picks one as a **victim** and throws **`SqlException 1205`** to that transaction, while letting the other continue.

This sets you up for the next prompts:
- First, observe the deadlock by calling `/api/orders/place` and `/api/orders/cancel` concurrently.
- Then add **retry logic** and later **consistent ordering** so deadlocks disappear in a controlled, production‑style way.


