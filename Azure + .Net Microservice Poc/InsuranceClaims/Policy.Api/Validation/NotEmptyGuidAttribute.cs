using System.ComponentModel.DataAnnotations;

namespace Policy.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute() : base("The {0} field must be a non-empty GUID.") { }

    public override bool IsValid(object? value) => value is Guid g && g != Guid.Empty;
}
