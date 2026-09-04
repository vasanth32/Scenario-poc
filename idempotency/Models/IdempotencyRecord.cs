namespace idempotency.Models;

public class IdempotencyRecord
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public string Status { get; set; } = "Processing";

    public int? WireRequestId { get; set; }

    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
