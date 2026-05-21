using FluentValidation;
using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Validators;

public sealed class CustomerJoinQueueRequestValidator : AbstractValidator<CustomerJoinQueueRequest>
{
    public CustomerJoinQueueRequestValidator()
    {
        RuleFor(request => request.CustomerName)
            .NotEmpty()
            .Must(NotBeWhiteSpace)
            .WithMessage("Customer name is required.")
            .MaximumLength(100);

        RuleFor(request => request.Mobile)
            .MaximumLength(30);

        RuleFor(request => request.PartySize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .When(request => request.PartySize.HasValue);

        RuleFor(request => request.ServiceReason)
            .MaximumLength(200);
    }

    private static bool NotBeWhiteSpace(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
