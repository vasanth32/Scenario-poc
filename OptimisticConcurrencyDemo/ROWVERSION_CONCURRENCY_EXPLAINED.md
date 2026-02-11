## Why We Needed RowVersion (The Problem)

- **Scenario**: Many clients can update the **same order row** at the same time.
- Without any concurrency control:
  - Client A reads Order `#1` → quantity = 1
  - Client B reads Order `#1` → quantity = 1
  - A sends update → quantity = 4
  - B sends update → quantity = 5
  - **Last write wins** silently (whichever `UPDATE` hits the DB last).
- **Problem**:  
  - A's work can be **overwritten** by B without anyone knowing.  
  - This is called a **lost update** and is the core concurrency issue we are solving.

We want the API to **notice** when another writer has already changed the row and either:
- retry safely, or
- fail with a clear **409 Conflict** instead of overwriting.

---

## What RowVersion Is

- In SQL Server, a `rowversion` (or old name `timestamp`) column:
  - Is a special **binary value** automatically managed by SQL Server.
  - Every time a row is updated, its `rowversion` value **changes**.
  - It is **unique and increasing** across the database.
- In EF Core, we map it like this:

```csharp
public class Order
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;

    [Timestamp]               // tells EF Core: this is a concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
```

- In `DbContext` we also configure it:

```csharp
modelBuilder.Entity<Order>(entity =>
{
    entity.HasKey(o => o.OrderId);
    entity.Property(o => o.Status).HasMaxLength(50);
    entity.Property(o => o.RowVersion).IsRowVersion(); // maps to SQL rowversion
});
```

Result in SQL (migration):
- Column `RowVersion` is:
  - type `rowversion`
  - marked as **concurrency token**
  - **auto-updated** on every `UPDATE`.

---

## How EF Core Uses RowVersion for Optimistic Concurrency

**Key idea**: EF Core includes the **original** `RowVersion` value in the `WHERE` clause of the `UPDATE`.

1. Client reads Order `#1`:
   - EF tracks:
     - `Quantity = 1`
     - `RowVersion = X` (some binary value from DB)
2. Client sends update: change quantity to 4.
3. EF generates SQL similar to:

```sql
UPDATE Orders
SET Quantity = 4
WHERE OrderId = 1 AND RowVersion = X;
```

4. Two cases:
   - **No one else changed the row**:
     - `RowVersion` is still `X`.
     - The `UPDATE` matches 1 row → success.
     - SQL also **changes RowVersion** from `X` to `Y`.
   - **Someone else already updated the row**:
     - DB row now has `RowVersion = Y` (different from X).
     - `WHERE OrderId = 1 AND RowVersion = X` matches **0 rows**.
     - EF sees "0 rows updated" and throws `DbUpdateConcurrencyException`.

This is called **optimistic concurrency**:
- We assume collisions are rare.
- We **detect** when they happen using the `RowVersion` check.

---

## How Our `UpdateOrder` Endpoint Uses This

Relevant code (simplified from `OrdersController.UpdateOrder`):

```csharp
const int maxRetries = 3;

for (var attempt = 1; attempt <= maxRetries; attempt++)
{
    var order = await _context.Orders
        .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

    if (order is null)
        return NotFound(...);

    ApplyUpdate(request, order);        // copy ProductId, Quantity, Status

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // demo only
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Order updated", attempt, ... });
    }
    catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
    {
        // Another writer won the race → reload latest DB values
        foreach (var entry in ex.Entries)
        {
            if (entry.Entity is not Order) continue;

            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
            if (databaseValues is null)
                return NotFound(...); // row was deleted

            entry.OriginalValues.SetValues(databaseValues);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken);
        // loop continues → we try again with refreshed RowVersion
    }
    catch (DbUpdateConcurrencyException)
    {
        // Even after retries, still conflicts → tell client to retry later
        return Conflict(new
        {
            message = "Order update failed due to concurrent updates. Please retry.",
            request.OrderId
        });
    }
}
```

### What this achieves

- If **no one else** touched the row:
  - `SaveChangesAsync` succeeds on **attempt 1**.
- If **another writer** updated the row between read and write:
  - `SaveChangesAsync` throws `DbUpdateConcurrencyException`.
  - We **catch** it, reload the latest DB values (including new `RowVersion`), and **retry** (up to 3 times).
- If the row keeps changing and we never win:
  - We **fail** with HTTP **409 Conflict** instead of overwriting someone else’s data.

So:
- **RowVersion detects** the conflict.
- **Our retry loop handles** transient conflicts.
- **409 Conflict** is the **safe fallback** when contention is too high.

---

## How the PowerShell Scripts Help You See This

We created two scripts under `OptimisticConcurrencyDemo/TestScripts`:

- `ParallelOrderUpdateTest.ps1`
  - Sends **two** concurrent `PUT /api/orders/update` requests (Request A and B).
  - Shows:
    - which one "wins" (final state),
    - and whether any needed retries.

- `BatchParallelUpdateTest.ps1`
  - Sends **10 concurrent** `PUT /api/orders/update` requests.
  - Some succeed on `attempt : 1`.
  - Some succeed on `attempt : 2` or `3` (after handling `DbUpdateConcurrencyException`).
  - Some end in **`(409) Conflict`** when they cannot win even after retries.

These scripts simulate many clients hitting the same order to show:
- **RowVersion + EF Core** detecting collisions (`DbUpdateConcurrencyException`),
- our **retry logic** recovering in many cases,
- and **409 Conflict** when it's safer to tell the caller to retry later.

---

## Summary in Plain Words

- The **issue**: multiple updates to the same order could silently overwrite each other (**lost updates**).
- The **tool**: `RowVersion`/`timestamp` column, mapped with `[Timestamp]`, tells EF Core to do **optimistic concurrency** using `WHERE RowVersion = X`.
- The **behavior**:
  - If someone else changed the row → `UPDATE` affects 0 rows → EF throws `DbUpdateConcurrencyException`.
  - Our code retries a few times with the latest values.
  - If we still can’t win → we return **409 Conflict** instead of silently losing or overwriting.

This is how `RowVersion` helps us **detect and safely handle** concurrency issues in the `OptimisticConcurrencyDemo` project.


