using FluentValidation;
using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Application.Features.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters")
            .EmailAddress()
            .WithMessage("Email format is invalid");
        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MaximumLength(128)
            .WithMessage("Password must not exceed 128 characters");
    }
}
