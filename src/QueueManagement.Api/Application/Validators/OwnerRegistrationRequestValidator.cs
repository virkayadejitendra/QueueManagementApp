using FluentValidation;
using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Validators;

public sealed class OwnerRegistrationRequestValidator : AbstractValidator<OwnerRegistrationRequest>
{
    public OwnerRegistrationRequestValidator()
    {
        RuleFor(request => request.OwnerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Mobile)
            .MaximumLength(30);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(request => request.BusinessName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.LocationName)
            .MaximumLength(150);

        RuleFor(request => request.Address)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(request => request.BusinessMobile)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.Email)
            .Must((request, _) => HasOwnerContact(request))
            .WithMessage("Either email or mobile is required.");

        RuleFor(request => request.Mobile)
            .Must((request, _) => HasOwnerContact(request))
            .WithMessage("Either email or mobile is required.");
    }

    private static bool HasOwnerContact(OwnerRegistrationRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            || !string.IsNullOrWhiteSpace(request.Mobile);
    }
}
