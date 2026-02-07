using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentDbContext context, ILogger<PaymentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/payments/process
    /// Process a payment
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<Payment>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        _logger.LogInformation("Processing payment for Order ID: {OrderId}, Amount: {Amount}", 
            request.OrderId, request.Amount);

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = "Processing",
            TransactionId = Guid.NewGuid().ToString()
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Simulate payment processing
        await Task.Delay(100); // Simulate external payment gateway call

        // Update payment status
        payment.Status = "Completed";
        payment.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment processed successfully. Payment ID: {PaymentId}, Transaction ID: {TransactionId}", 
            payment.Id, payment.TransactionId);

        return Ok(payment);
    }

    /// <summary>
    /// GET /api/payments/{id}
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        _logger.LogInformation("Getting payment with ID: {PaymentId}", id);

        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            _logger.LogWarning("Payment with ID {PaymentId} not found", id);
            return NotFound();
        }

        return Ok(payment);
    }
}

public class ProcessPaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

