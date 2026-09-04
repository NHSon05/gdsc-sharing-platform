using System.Text.RegularExpressions;
using FluentValidation;
using GdscSharingPlatform.Application.Features.Memberships.Models;

namespace GdscSharingPlatform.Application.Features.Memberships.Validators;

public sealed class CreateGenerationRequestValidator : AbstractValidator<CreateGenerationRequest>
{
    public CreateGenerationRequestValidator()
    {
        RuleFor(x => x.Number)
            .GreaterThan(0)
            .WithMessage("Generation number must be greater than 0.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate.Value <= x.EndDate.Value)
            .WithMessage("Start date must be before or equal to end date.");
    }
}

public sealed class UpdateGenerationRequestValidator : AbstractValidator<UpdateGenerationRequest>
{
    public UpdateGenerationRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate.Value <= x.EndDate.Value)
            .WithMessage("Start date must be before or equal to end date.");
    }
}

public sealed class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Department name is required.")
            .MaximumLength(100)
            .WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Department slug is required.")
            .MaximumLength(100)
            .WithMessage("Department slug cannot exceed 100 characters.")
            .Matches(SlugRegex)
            .WithMessage("Slug must be lowercase alphanumeric characters and hyphens.");

        When(x => !string.IsNullOrWhiteSpace(x.Color), () =>
        {
            RuleFor(x => x.Color)
                .MaximumLength(20)
                .Matches(HexColorRegex)
                .WithMessage("Color must be a valid hex color code (e.g. #3B82F6).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Icon), () =>
        {
            RuleFor(x => x.Icon)
                .MaximumLength(100)
                .WithMessage("Icon key cannot exceed 100 characters.");
        });

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be greater than or equal to 0.");
    }
}

public sealed class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Department name is required.")
            .MaximumLength(100)
            .WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Department slug is required.")
            .MaximumLength(100)
            .WithMessage("Department slug cannot exceed 100 characters.")
            .Matches(SlugRegex)
            .WithMessage("Slug must be lowercase alphanumeric characters and hyphens.");

        When(x => !string.IsNullOrWhiteSpace(x.Color), () =>
        {
            RuleFor(x => x.Color)
                .MaximumLength(20)
                .Matches(HexColorRegex)
                .WithMessage("Color must be a valid hex color code (e.g. #3B82F6).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Icon), () =>
        {
            RuleFor(x => x.Icon)
                .MaximumLength(100)
                .WithMessage("Icon key cannot exceed 100 characters.");
        });

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be greater than or equal to 0.");
    }
}

public sealed class AssignMemberToGenRequestValidator : AbstractValidator<AssignMemberToGenRequest>
{
    public AssignMemberToGenRequestValidator()
    {
        RuleFor(x => x.GenerationId)
            .NotEmpty()
            .WithMessage("GenerationId is required.");
    }
}

public sealed class AddMemberToDepartmentRequestValidator : AbstractValidator<AddMemberToDepartmentRequest>
{
    public AddMemberToDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("DepartmentId is required.");

        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(roles => roles.Count > 0)
            .WithMessage("At least one role must be assigned.");
    }
}

public sealed class ReplaceRolesRequestValidator : AbstractValidator<ReplaceRolesRequest>
{
    public ReplaceRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotNull()
            .Must(roles => roles.Count > 0)
            .WithMessage("At least one role must be provided.");
    }
}
