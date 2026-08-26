using FluentValidation;
using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Application.Features.Auth.Validators;

public sealed class LogoutRequestValidator
    : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MaximumLength(2048)
            .WithMessage(
                "Refresh token must not exceed 2048 characters.");
    }
}