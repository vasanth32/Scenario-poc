using Microsoft.EntityFrameworkCore;
using OptimisticConcurrencyDemo.Models;

namespace OptimisticConcurrencyDemo.Data;

public static class OptimisticConcurrencyDemoSeeder
{
    public static void Seed(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OptimisticConcurrencyDemoDbContext>();

        context.Database.Migrate();

        if (context.Orders.Any())
        {
            return;
        }

        context.Orders.Add(new Order
        {
            ProductId = 101,
            Quantity = 2,
            Status = "Placed"
        });

        context.SaveChanges();
    }
}

