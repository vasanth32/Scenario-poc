namespace idempotency.DTOs;

public class CreateWireRequest
{
    public string FromAccount { get; set; } = string.Empty;

    public string ToAccount { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;
}
