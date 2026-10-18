using System.ComponentModel.DataAnnotations;
using Claims.Api.Validation;

namespace Claims.Api.Models;

public enum ClaimStatus { Submitted, UnderReview, Approved, Rejected }

// Entity (persisted in ClaimsDb, never returned directly).
// PolicyId is a plain value: the policy lives in Policy.Api's database, so there is no foreign key.
public class Claim
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Notes { get; set; }
}

// Response DTO
public record ClaimDto(Guid Id, Guid PolicyId, string Description, decimal Amount,
    string Status, DateTime CreatedAtUtc, string? Notes = null)
{
    public static ClaimDto From(Claim c) =>
        new(c.Id, c.PolicyId, c.Description, c.Amount, c.Status.ToString(), c.CreatedAtUtc, c.Notes);
}

// Request DTOs
public record CreateClaimRequest(
    [NotEmptyGuid] Guid PolicyId,
    [Required, StringLength(500)] string Description,
    [Range(typeof(decimal), "0.01", "1000000000")] decimal Amount,
    [StringLength(1000)] string? Notes = null);

public record UpdateClaimRequest(
    [Required, StringLength(500)] string Description,
    [Range(typeof(decimal), "0.01", "1000000000")] decimal Amount,
    [EnumDataType(typeof(ClaimStatus))] ClaimStatus Status,
    [StringLength(1000)] string? Notes = null);
