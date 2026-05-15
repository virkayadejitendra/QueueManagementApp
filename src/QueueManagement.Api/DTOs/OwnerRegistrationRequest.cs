using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Api.DTOs;

public sealed class OwnerRegistrationRequest : IValidatableObject
{
    [Required]
    [MaxLength(100)]
    public string OwnerName { get; init; } = string.Empty;

    [EmailAddress]
    [MaxLength(254)]
    public string? Email { get; init; }

    [MaxLength(30)]
    public string? Mobile { get; init; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string BusinessName { get; init; } = string.Empty;

    [MaxLength(150)]
    public string? LocationName { get; init; }

    [Required]
    [MaxLength(300)]
    public string Address { get; init; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string BusinessMobile { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var validationResult in RequiredText(OwnerName, nameof(OwnerName)))
        {
            yield return validationResult;
        }

        foreach (var validationResult in RequiredText(Password, nameof(Password)))
        {
            yield return validationResult;
        }

        foreach (var validationResult in RequiredText(BusinessName, nameof(BusinessName)))
        {
            yield return validationResult;
        }

        foreach (var validationResult in RequiredText(Address, nameof(Address)))
        {
            yield return validationResult;
        }

        foreach (var validationResult in RequiredText(BusinessMobile, nameof(BusinessMobile)))
        {
            yield return validationResult;
        }

        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Mobile))
        {
            yield return new ValidationResult(
                "Either email or mobile is required.",
                [nameof(Email), nameof(Mobile)]);
        }
    }

    private static IEnumerable<ValidationResult> RequiredText(string value, string memberName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield return new ValidationResult(
                $"{memberName} is required.",
                [memberName]);
        }
    }
}
