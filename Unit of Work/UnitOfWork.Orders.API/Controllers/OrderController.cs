using Microsoft.AspNetCore.Mvc;
using UnitOfWork.Orders.Application;
using UnitOfWork.Orders.Domain;

namespace UnitOfWork.Orders.API.Controllers;

/// <summary>
/// Order Controller - Demonstrates Unit of Work Pattern
/// 
/// This controller uses UnitOfWork to ensure transaction consistency.
/// When creating an order with payment, both are saved in ONE transaction.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderPaymentService _orderPaymentService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(OrderPaymentService orderPaymentService, IUnitOfWork unitOfWork)
    {
        _orderPaymentService = orderPaymentService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// POST: api/order/create-with-payment
    /// Create Order and Payment in a single transaction
    /// 
    /// This demonstrates Unit of Work:
    /// - Both Order and Payment are added to repositories
    /// - UnitOfWork.CommitAsync() saves BOTH in ONE transaction
    /// - If payment fails, order is also rolled back
    /// </summary>
    [HttpPost("create-with-payment")]
    public async Task<ActionResult> CreateOrderWithPayment([FromBody] CreateOrderWithPaymentRequest request)
    {
        try
        {
            var (order, payment) = await _orderPaymentService.CreateOrderWithPaymentAsync(
                request.ProductName,
                request.Quantity,
                request.Price,
                request.PaymentMethod
            );

            return Ok(new
            {
                message = "Order and Payment created successfully in one transaction",
                order,
                payment
            });
        }
        catch (Exception ex)
        {
            // If anything fails, UnitOfWork ensures both are rolled back
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/order
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// GET: api/order/payments
    /// Get all payments
    /// </summary>
    [HttpGet("payments")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
    {
        var payments = await _unitOfWork.Payments.GetAllAsync();
        return Ok(payments);
    }
}

/// <summary>
/// DTO for creating order with payment
/// </summary>
public class CreateOrderWithPaymentRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

