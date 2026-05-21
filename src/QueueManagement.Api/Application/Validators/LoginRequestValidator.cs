using FluentValidation;
using QueueManagement.Api.Application.DTOs;

namespace QueueManagement.Api.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Identifier)
            .NotEmpty()
            .MaximumLength(254);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(100);
    }
}
