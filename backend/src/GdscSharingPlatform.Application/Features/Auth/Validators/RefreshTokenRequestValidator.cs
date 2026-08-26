using FluentValidation;
using GdscSharingPlatform.Application.Features.Auth.Models;

namespace GdscSharingPlatform.Application.Features.Auth.Validators;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("RefreshToken is required")
            .MaximumLength(2048)
            .WithMessage("Refresh token must not exceed 2048 characters");
    }
}
