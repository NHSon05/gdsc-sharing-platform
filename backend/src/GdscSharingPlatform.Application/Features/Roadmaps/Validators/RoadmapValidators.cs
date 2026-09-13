using FluentValidation;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Validators;

internal static class RoadmapInputRules
{
    public const decimal MaxCoordinate = 99999999999999.9999m;
    public static bool Slug(string? value)
    {
        try { _ = RoadmapValidation.Slug(value!); return true; }
        catch (ArgumentException) { return false; }
    }
    public static bool HttpUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !string.IsNullOrEmpty(uri.Host);
}

public sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Must(RoadmapInputRules.Slug).WithMessage("Slug must normalize to 1–150 URL characters.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Icon).MaximumLength(2048);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class RoadmapRequestValidator : AbstractValidator<RoadmapRequest>
{
    public RoadmapRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Must(RoadmapInputRules.Slug);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(100000);
        RuleFor(x => x.Prerequisites).MaximumLength(100000);
        RuleFor(x => x.ThumbnailUrl).MaximumLength(2048)
            .Must(x => x is null || RoadmapInputRules.HttpUrl(x) || (x.StartsWith('/') && !x.StartsWith("//") && !x.Contains('\\')))
            .WithMessage("Thumbnail must be an HTTP(S) URL or a local absolute path.");
        RuleFor(x => x.EstimatedDuration).MaximumLength(100);
        RuleFor(x => x.Level).IsInEnum();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class NodeRequestValidator : AbstractValidator<NodeRequest>
{
    public NodeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Must(RoadmapInputRules.Slug);
        RuleFor(x => x.NodeType).IsInEnum();
        RuleFor(x => x.PositionX).InclusiveBetween(-RoadmapInputRules.MaxCoordinate, RoadmapInputRules.MaxCoordinate);
        RuleFor(x => x.PositionY).InclusiveBetween(-RoadmapInputRules.MaxCoordinate, RoadmapInputRules.MaxCoordinate);
        RuleFor(x => x.Width).InclusiveBetween(0.0001m, RoadmapInputRules.MaxCoordinate).When(x => x.Width.HasValue);
        RuleFor(x => x.Description).MaximumLength(100000);
        RuleFor(x => x.LearningObjectives).MaximumLength(100000);
        RuleFor(x => x.EstimatedDuration).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Icon).MaximumLength(2048);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class EdgeRequestValidator : AbstractValidator<EdgeRequest>
{
    public EdgeRequestValidator()
    {
        RuleFor(x => x.SourceNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId).NotEmpty().NotEqual(x => x.SourceNodeId).WithMessage("Self-loops are not allowed.");
        RuleFor(x => x.RelationType).IsInEnum();
        RuleFor(x => x.LineStyle).IsInEnum();
        RuleFor(x => x.LineStyle).Equal(RoadmapLineStyle.Solid).When(x => x.RelationType == RoadmapRelationType.Required);
        RuleFor(x => x.Label).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class LinkResourceRequestValidator : AbstractValidator<LinkResourceRequest>
{
    public LinkResourceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ExternalUrl).NotEmpty().MaximumLength(2048).Must(RoadmapInputRules.HttpUrl).WithMessage("Only valid HTTP(S) URLs are accepted.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class FileResourceRequestValidator : AbstractValidator<FileResourceRequest>
{
    public FileResourceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class UpdateResourceRequestValidator : AbstractValidator<UpdateResourceRequest>
{
    public UpdateResourceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ExternalUrl).MaximumLength(2048).Must(RoadmapInputRules.HttpUrl).When(x => x.ExternalUrl is not null);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class ActiveStatusRequestValidator : AbstractValidator<ActiveStatusRequest>
{
    public ActiveStatusRequestValidator() => RuleFor(x => x.IsActive).NotNull();
}
public sealed class RoadmapStatusRequestValidator : AbstractValidator<RoadmapStatusRequest>
{
    public RoadmapStatusRequestValidator() => RuleFor(x => x.Status).NotNull().IsInEnum();
}
public sealed class ReorderRequestValidator : AbstractValidator<ReorderRequest>
{
    public ReorderRequestValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().Must(x => x is null || x.Count <= 10000).WithMessage("At most 10000 IDs are allowed.")
            .Must(x => x is null || x.Distinct().Count() == x.Count).WithMessage("IDs must be unique.");
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
public sealed class NodePositionsRequestValidator : AbstractValidator<NodePositionsRequest>
{
    public NodePositionsRequestValidator()
    {
        RuleFor(x => x.Nodes).NotEmpty().Must(x => x is null || x.Count <= 10000)
            .Must(x => x is null || x.Where(n => n is not null).Select(n => n.Id).Distinct().Count() == x.Count)
            .WithMessage("Node IDs must be unique and entries must not be null.");
        RuleForEach(x => x.Nodes).NotNull().ChildRules(node =>
        {
            node.RuleFor(x => x.Id).NotEmpty();
            node.RuleFor(x => x.PositionX).InclusiveBetween(-RoadmapInputRules.MaxCoordinate, RoadmapInputRules.MaxCoordinate);
            node.RuleFor(x => x.PositionY).InclusiveBetween(-RoadmapInputRules.MaxCoordinate, RoadmapInputRules.MaxCoordinate);
        });
    }
}
public sealed class RoadmapQueryValidator : AbstractValidator<RoadmapQuery>
{
    public RoadmapQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(150);
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty).When(x => x.CategoryId.HasValue);
        RuleFor(x => x.Level).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Page).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
