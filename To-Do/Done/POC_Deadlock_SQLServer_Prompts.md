## .NET + SQL Server Deadlock POC – Cursor Prompt Script

Great idea — doing a **POC (Proof of Concept)** will make deadlocks very clear.

Below are **ready-to-use Cursor AI prompts** to build a **.NET + SQL Server deadlock simulation POC** step-by-step.

You can paste each prompt sequentially in Cursor.

---

## Prompt 1 — Create Project

```text
Create a .NET 8 Web API project named DeadlockDemo using Entity Framework Core with SQL Server.

Add two tables:
1. Orders
   - OrderId (int, PK)
   - ProductId (int)
   - Quantity (int)
   - Status (nvarchar(50))

2. Inventory
   - ProductId (int, PK)
   - AvailableQty (int)
   - ReservedQty (int)

Configure DbContext and connection string.
Add migration and database update commands.
```

---

## Prompt 2 — Seed Data

```text
Create a data seeder that inserts:
Orders:
OrderId = 1, ProductId = 101, Quantity = 2, Status = 'Placed'

Inventory:
ProductId = 101, AvailableQty = 100, ReservedQty = 0
```

---

## Prompt 3 — Deadlock API 1 (PlaceOrder)

```text
Create an API endpoint:
POST /api/orders/place

Inside a database transaction:

Step 1:
Update Inventory table first
ReservedQty += Quantity

Wait 5 seconds (simulate delay)

Step 2:
Update Orders table
Status = 'PlacedAgain'

Commit transaction.
```

---

## Prompt 4 — Deadlock API 2 (CancelOrder)

```text
Create another API endpoint:
POST /api/orders/cancel

Inside a database transaction:

Step 1:
Update Orders table first
Status = 'Cancelled'

Wait 5 seconds (simulate delay)

Step 2:
Update Inventory table
ReservedQty -= Quantity

Commit transaction.
```

---

## Prompt 5 — Deadlock Simulation Controller

```text
Create an endpoint:
POST /api/test/deadlock

Inside this endpoint, call both APIs simultaneously using Task.WhenAll to trigger concurrent execution so that a deadlock occurs.
Return the result showing if deadlock exception occurs.
```

---

## Prompt 6 — Retry Logic Implementation

```text
Modify both APIs to include retry logic:
If SqlException number 1205 (deadlock) occurs, retry operation up to 3 times before failing.
Add logging to show retry attempts.
```

---

## Expected Learning Outcome

After running:

* First execution → deadlock error
* After retry logic → operation succeeds
* After fixing table update order → deadlock disappears permanently

This gives **complete real-time understanding**.


