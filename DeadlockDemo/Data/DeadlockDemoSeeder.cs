using DeadlockDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace DeadlockDemo.Data;

/// <summary>
/// Simple data seeder for the SQL deadlock POC.
/// Inserts one Order and one Inventory row if database is empty.
/// </summary>
public static class DeadlockDemoSeeder
{
    public static void Seed(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeadlockDemoDbContext>();

        // Ensure database and schema exist (applies pending migrations)
        context.Database.Migrate();

        // If there is already data, don't seed again
        if (context.Orders.Any() || context.Inventory.Any())
        {
            return;
        }

        // Seed initial order (let SQL Server generate OrderId)
        var order = new Order
        {
            ProductId = 101,
            Quantity = 2,
            Status = "Placed"
        };

        // Seed initial inventory
        var inventory = new Inventory
        {
            ProductId = 101,
            AvailableQty = 100,
            ReservedQty = 0
        };

        context.Orders.Add(order);
        context.Inventory.Add(inventory);

        context.SaveChanges();
    }
}


