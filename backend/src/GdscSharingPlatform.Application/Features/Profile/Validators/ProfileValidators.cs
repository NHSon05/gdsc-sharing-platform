using System.Text.RegularExpressions;
using FluentValidation;
using GdscSharingPlatform.Application.Features.Profile.Models;

namespace GdscSharingPlatform.Application.Features.Profile.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s\-().]{8,20}$", RegexOptions.Compiled);

    public UpdateProfileRequestValidator()
    {
        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName)
                .MinimumLength(2)
                .WithMessage("Display name must be at least 2 characters.")
                .MaximumLength(150)
                .WithMessage("Display name cannot exceed 150 characters.")
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Display name cannot be only whitespace.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .MaximumLength(256)
                .WithMessage("Email cannot exceed 256 characters.")
                .EmailAddress()
                .WithMessage("Email address is invalid.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters.")
                .Matches(PhoneRegex)
                .WithMessage("Phone number format is invalid.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.StudentCode), () =>
        {
            RuleFor(x => x.StudentCode)
                .MinimumLength(3)
                .WithMessage("Student code must be at least 3 characters.")
                .MaximumLength(30)
                .WithMessage("Student code cannot exceed 30 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.GithubUrl), () =>
        {
            RuleFor(x => x.GithubUrl)
                .MaximumLength(200)
                .WithMessage("GitHub URL cannot exceed 200 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                             uri.Scheme == Uri.UriSchemeHttps &&
                             (uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                              uri.Host.EndsWith(".github.com", StringComparison.OrdinalIgnoreCase)))
                .WithMessage("GitHub URL must be a valid HTTPS URL pointing to github.com.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Bio), () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(500)
                .WithMessage("Bio cannot exceed 500 characters.");
        });
    }
}

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .WithMessage("New email is required.")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters.")
            .EmailAddress()
            .WithMessage("Email address is invalid.");
    }
}
