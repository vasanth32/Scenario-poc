## DeadlockDemo – SQL Deadlock POC (Prompt 1)

This project is a **.NET 8 Web API** used to simulate SQL Server deadlocks.

### Setup Done (from Prompt 1)

- Created Web API project: `DeadlockDemo`
- Added **Entity Framework Core with SQL Server**:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Design`
- Defined entities:
  - `Orders` – `OrderId` (PK), `ProductId`, `Quantity`, `Status`
  - `Inventory` – `ProductId` (PK), `AvailableQty`, `ReservedQty`
- Configured `DeadlockDemoDbContext` with both tables and primary keys.
- Configured connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\\\MSSQLLocalDB;Database=DeadlockDemoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### EF Core Migration & Database Update Commands

Run these from the `DeadlockDemo` folder:

```bash
dotnet tool install --global dotnet-ef        # if you don't have it yet (run once)

dotnet ef migrations add InitialCreate        # create migration
dotnet ef database update                     # apply migration to SQL Server
```

After this, the `Orders` and `Inventory` tables will be created in the `DeadlockDemoDb` database.

Next steps (from later prompts):
- Add seeding data.
- Create APIs that intentionally cause a deadlock and then add retry logic.


