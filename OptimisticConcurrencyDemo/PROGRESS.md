# OptimisticConcurrencyDemo - Progress

## Goal

Build a .NET 8 Web API POC to demonstrate optimistic concurrency with EF Core + SQL Server using a `rowversion` column.

## Completed Work

- Created project: `OptimisticConcurrencyDemo`
- Added EF Core packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Design`
- Configured SQL Server connection in `appsettings.json`:
  - Server: `VASANTH\SQLEXPRESS`
  - Database: `OptimisticConcurrencyDemoDb`
- Added `Order` model with:
  - `OrderId` (PK)
  - `ProductId`
  - `Quantity`
  - `Status`
  - `RowVersion` (`byte[]`) with `[Timestamp]`
- Added `OptimisticConcurrencyDemoDbContext` and mapped `RowVersion` as SQL `rowversion`.
- Replaced template startup with:
  - Controllers
  - DbContext registration
  - Seeder invocation at startup
- Added `OptimisticConcurrencyDemoSeeder`:
  - Applies migrations
  - Seeds one initial order if table is empty
- Added endpoint `PUT /api/orders/update` with retry logic for `DbUpdateConcurrencyException`.
- Added endpoint `GET /api/orders/{orderId}` for quick verification.
- Added HTTP test requests in `OptimisticConcurrencyDemo.http`.
- Created migration:
  - `InitialCreate` for `Orders` table including `RowVersion`.

## How Concurrency Is Handled

- `RowVersion` is a concurrency token.
- EF Core includes the previous `RowVersion` in the `UPDATE` `WHERE` clause.
- If another request updates the same row first, SQL updates 0 rows.
- EF Core throws `DbUpdateConcurrencyException`.
- The API catches the exception, reloads current DB values, and retries up to 3 times.
- If all retries fail, API returns `409 Conflict`.

## Run and Test

From `OptimisticConcurrencyDemo` folder:

```powershell
dotnet build
dotnet run
```

Use requests in `OptimisticConcurrencyDemo.http`:

1. `GET /api/orders/1` to see seeded data.
2. Send two `PUT /api/orders/update` requests for same `orderId` close together.
3. Observe success/retry behavior. One call may retry on conflict.

## Current Status

- Implementation complete for prompt items 25-38.
- Build successful.
- Ready for manual concurrency testing.

