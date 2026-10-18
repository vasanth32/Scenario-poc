using System.ComponentModel.DataAnnotations;

namespace Policy.Api.Models;

// Entity (persisted in PolicyDb, never returned directly)
public class PolicyEntity
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = "";
    public string HolderName { get; set; } = "";
    public decimal CoverageAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}

// Response DTO
public record PolicyDto(Guid Id, string PolicyNumber, string HolderName, decimal CoverageAmount,
    DateOnly StartDate, DateOnly EndDate, bool IsActive)
{
    public static PolicyDto From(PolicyEntity p) =>
        new(p.Id, p.PolicyNumber, p.HolderName, p.CoverageAmount, p.StartDate, p.EndDate, p.IsActive);
}

// Request DTOs
public record CreatePolicyRequest(
    [Required, StringLength(50)] string PolicyNumber,
    [Required, StringLength(200)] string HolderName,
    [Range(typeof(decimal), "0.01", "1000000000")] decimal CoverageAmount,
    DateOnly StartDate,
    DateOnly EndDate) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("EndDate must be after StartDate.", [nameof(EndDate)]);
    }
}

public record UpdatePolicyRequest(
    [Required, StringLength(50)] string PolicyNumber,
    [Required, StringLength(200)] string HolderName,
    [Range(typeof(decimal), "0.01", "1000000000")] decimal CoverageAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("EndDate must be after StartDate.", [nameof(EndDate)]);
    }
}
