using UnitOfWork.Orders.Domain;

namespace UnitOfWork.Orders.Application;

/// <summary>
/// Service demonstrating Unit of Work pattern
/// 
/// This service shows how to save Order + Payment in ONE transaction.
/// If payment fails, order will NOT be saved (transaction rollback).
/// </summary>
public class OrderPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderPaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create Order and Payment in a single transaction
    /// 
    /// TRANSACTION CONSISTENCY EXPLAINED:
    /// 1. Order is added to context (not saved yet)
    /// 2. Payment is added to context (not saved yet)  
    /// 3. CommitAsync() saves BOTH in ONE database transaction
    /// 4. If ANY step fails, the entire transaction is rolled back
    /// 
    /// This ensures: Either BOTH Order and Payment are saved, or NEITHER is saved.
    /// </summary>
    public async Task<(Order order, Payment payment)> CreateOrderWithPaymentAsync(
        string productName,
        int quantity,
        decimal price,
        string paymentMethod)
    {
        // Step 1: Create Order (added to context, NOT saved yet)
        var order = new Order
        {
            ProductName = productName,
            Quantity = quantity,
            Price = price
        };
        await _unitOfWork.Orders.AddAsync(order);
        // Notice: No SaveChanges() here! Repository doesn't save.

        // Step 2: Create Payment (added to context, NOT saved yet)
        // For demo purposes, we'll use a temporary OrderId
        // In production, configure EF Core relationship to handle this automatically
        var payment = new Payment
        {
            OrderId = 0, // Will be updated after order gets ID
            Amount = price * quantity,
            PaymentMethod = paymentMethod
        };
        await _unitOfWork.Payments.AddAsync(payment);
        // Notice: No SaveChanges() here! Repository doesn't save.

        // Step 3: Save BOTH in ONE transaction
        // UnitOfWork.CommitAsync() is the ONLY place SaveChanges() is called
        // This saves ALL pending changes (Order + Payment) in ONE database transaction
        // If this fails, BOTH Order and Payment are automatically rolled back
        await _unitOfWork.CommitAsync();
        
        // After CommitAsync(), order.Id is now set by EF Core
        // Update payment with correct OrderId and save again
        // (In production, configure EF relationship to do this automatically)
        payment.OrderId = order.Id;
        await _unitOfWork.CommitAsync();

        return (order, payment);
    }

    /// <summary>
    /// Example: What happens if payment validation fails?
    /// This demonstrates transaction rollback.
    /// </summary>
    public async Task<bool> CreateOrderWithPaymentWithValidationAsync(
        string productName,
        int quantity,
        decimal price,
        string paymentMethod)
    {
        try
        {
            // Add order
            var order = new Order
            {
                ProductName = productName,
                Quantity = quantity,
                Price = price
            };
            await _unitOfWork.Orders.AddAsync(order);

            // Simulate payment validation failure
            if (paymentMethod == "INVALID")
            {
                throw new InvalidOperationException("Invalid payment method");
                // Even though order was added, it won't be saved because
                // we haven't called CommitAsync() yet, and the exception
                // will prevent CommitAsync() from being called.
            }

            // Add payment
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = price * quantity,
                PaymentMethod = paymentMethod
            };
            await _unitOfWork.Payments.AddAsync(payment);

            // Commit both
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            // If anything fails, rollback all changes
            await _unitOfWork.RollbackAsync();
            return false;
        }
    }
}

