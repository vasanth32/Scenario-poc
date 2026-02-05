using SOLID.Orders.Domain;

namespace SOLID.Orders.Application;

/// <summary>
/// SOLID Principle: SINGLE RESPONSIBILITY
/// ======================================
/// "A class should have only one reason to change"
/// 
/// This OrderService has ONE responsibility: Orchestrating order creation.
/// It doesn't:
/// - Validate orders (delegated to IOrderValidator)
/// - Calculate totals (delegated to IOrderCalculator)
/// - Calculate discounts (delegated to IDiscountCalculator)
/// - Save orders (delegated to IOrderRepository)
/// - Process payments (delegated to IPaymentProcessor)
/// 
/// If validation logic changes → Change OrderValidator, not OrderService
/// If calculation logic changes → Change OrderCalculator, not OrderService
/// If discount logic changes → Change DiscountCalculator, not OrderService
/// 
/// Benefits:
/// - Easy to understand (does one thing)
/// - Easy to test (fewer dependencies per test)
/// - Easy to maintain (changes are isolated)
/// - Follows Single Responsibility Principle
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IOrderValidator _validator;
    private readonly IOrderCalculator _calculator;
    private readonly IDiscountCalculator _discountCalculator;

    public OrderService(
        IOrderRepository repository,
        IOrderValidator validator,
        IOrderCalculator calculator,
        IDiscountCalculator discountCalculator)
    {
        _repository = repository;
        _validator = validator;
        _calculator = calculator;
        _discountCalculator = discountCalculator;
    }

    /// <summary>
    /// Single Responsibility: This method orchestrates order creation.
    /// It delegates specific tasks to specialized services.
    /// </summary>
    public async Task<Order> CreateOrderAsync(string productName, int quantity, decimal price)
    {
        // Delegate validation to validator
        if (!_validator.Validate(price, quantity))
        {
            throw new ArgumentException("Invalid order data");
        }

        // Delegate discount calculation to discount calculator
        var discount = _discountCalculator.CalculateDiscount(price * quantity);

        // Delegate total calculation to calculator
        var total = _calculator.CalculateTotal(price, quantity, discount);

        // Create order
        var order = new Order
        {
            ProductName = productName,
            Quantity = quantity,
            Price = price,
            DiscountAmount = discount,
            TotalAmount = total
        };

        // Delegate persistence to repository
        return await _repository.AddAsync(order);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

