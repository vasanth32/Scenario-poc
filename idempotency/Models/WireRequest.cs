namespace idempotency.Models;

public class WireRequest
{
    public int Id { get; set; }

    public string FromAccount { get; set; } = string.Empty;

    public string ToAccount { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = "Created";

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
