using FluentValidation;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;

namespace GdscSharingPlatform.Application.Features.Sharing;

public static class SharingRules
{
    public static bool HttpUrl(string? value) => value is not null && value.Length <= 2048
        && Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
        && !string.IsNullOrEmpty(uri.Host) && string.IsNullOrEmpty(uri.UserInfo) && !value.Any(char.IsControl);

    public static bool CanRead(SharingContent content, Guid userId, bool admin) => admin
        || content.Status == SharingContentStatus.Published
        || content.Authors.Any(a => a.UserId == userId && (a.AuthorRole == SharingAuthorRole.Owner
            || content.Status != SharingContentStatus.Draft));

    public static void RequireEdit(SharingContent content, Guid userId, bool admin)
    {
        var owner = content.Authors.Any(a => a.UserId == userId && a.AuthorRole == SharingAuthorRole.Owner);
        var contributor = content.Authors.Any(a => a.UserId == userId && a.AuthorRole == SharingAuthorRole.Contributor);
        if (!owner && !admin && !(contributor && content.Status == SharingContentStatus.Rejected))
            throw new ForbiddenAccessException();
        if (content.Status is SharingContentStatus.PendingReview or SharingContentStatus.Archived)
            throw new ApplicationValidationException("Content is locked in this status.");
        if (content.Status == SharingContentStatus.Published && !owner)
            throw new ForbiddenAccessException("Only the Owner may edit Published content directly.");
    }

    public static DateTimeOffset ToUtc(DateTime local, string timeZoneId, string fieldName = "startsAtLocal")
    {
        if (local.Kind != DateTimeKind.Unspecified)
            throw new ApplicationValidationException(fieldName, "Local times must not contain a UTC marker or offset.");
        try
        {
            // Persist the documented IANA identifier; reject platform-specific Windows zone names.
            if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out _) && timeZoneId != "UTC")
                throw new TimeZoneNotFoundException();
            var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
                throw new ApplicationValidationException(fieldName, "Local time is missing or ambiguous due to daylight saving.");
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone));
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        { throw new ApplicationValidationException("timeZoneId", "Choose a valid IANA time zone."); }
    }
}

public sealed class ContentRequestValidator : AbstractValidator<ContentRequest>
{
    public ContentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9]+(-[a-z0-9]+)*$");
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.BodyMarkdown).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.CoverImageUrl).Must(x => x is null || SharingRules.HttpUrl(x));
        RuleFor(x => x.TagIds).NotNull().Must(ValidIds);
        RuleFor(x => x.ContributorUserIds).NotNull().Must(ValidIds);
    }
    internal static bool ValidIds(IReadOnlyList<Guid>? ids) => ids is not null && ids.Count <= 100
        && ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count;
}
public sealed class ResourceRequestValidator : AbstractValidator<ResourceRequest>
{
    public ResourceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ExternalUrl).Must(x => x is null || SharingRules.HttpUrl(x));
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class TagRequestValidator : AbstractValidator<TagRequest>
{
    public TagRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9]+(-[a-z0-9]+)*$");
        RuleFor(x => x.Color).Matches("^#[0-9a-fA-F]{6}$").When(x => x.Color is not null);
    }
}
public sealed class TagStatusRequestValidator : AbstractValidator<TagStatusRequest>
{ public TagStatusRequestValidator() => RuleFor(x => x.IsActive).NotNull(); }
public sealed class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{ public ReviewRequestValidator() => RuleFor(x => x.ReviewNote).NotEmpty().MaximumLength(4000); }
public sealed class CancelScheduleRequestValidator : AbstractValidator<CancelScheduleRequest>
{ public CancelScheduleRequestValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(4000); }
public sealed class ReorderResourcesRequestValidator : AbstractValidator<ReorderResourcesRequest>
{ public ReorderResourcesRequestValidator() => RuleFor(x => x.Ids).NotNull().Must(ContentRequestValidator.ValidIds); }
public sealed class ScheduleContentsRequestValidator : AbstractValidator<ScheduleContentsRequest>
{ public ScheduleContentsRequestValidator() => RuleFor(x => x.ContentIds).NotNull().Must(ContentRequestValidator.ValidIds); }
public sealed class PresenterRequestValidator : AbstractValidator<PresenterRequest>
{
    public PresenterRequestValidator()
    { RuleFor(x => x.UserId).NotEmpty(); RuleFor(x => x.Role).IsInEnum(); RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0); }
}
public sealed class PresentersRequestValidator : AbstractValidator<PresentersRequest>
{
    public PresentersRequestValidator()
    {
        RuleFor(x => x.Presenters).NotNull().Must(x => x is not null && x.Count <= 100 && x.All(p => p is not null)
            && x.Select(p => (p.UserId, p.Role)).Distinct().Count() == x.Count);
        RuleForEach(x => x.Presenters).NotNull().SetValidator(new PresenterRequestValidator());
    }
}
public sealed class AudienceRequestValidator : AbstractValidator<AudienceRequest>
{
    public AudienceRequestValidator()
    {
        RuleFor(x => x.AudienceScope).IsInEnum();
        RuleFor(x => x.GenerationIds).NotNull().Must(ContentRequestValidator.ValidIds);
        RuleFor(x => x.DepartmentIds).NotNull().Must(ContentRequestValidator.ValidIds);
        RuleFor(x => x).Must(x => x.GenerationIds is not null && x.DepartmentIds is not null &&
            (x.AudienceScope == AudienceScope.AllMembers ? x.GenerationIds.Count + x.DepartmentIds.Count == 0
                : x.GenerationIds.Count + x.DepartmentIds.Count > 0)).WithMessage("Select at least one audience, or clear targets for AllMembers.");
    }
}
public sealed class ScheduleRequestValidator : AbstractValidator<ScheduleRequest>
{
    public ScheduleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(100_000);
        RuleFor(x => x.SharingType).IsInEnum(); RuleFor(x => x.DeliveryMode).IsInEnum();
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartsAtLocal).NotEmpty(); RuleFor(x => x.EndsAtLocal).GreaterThan(x => x.StartsAtLocal);
        RuleFor(x => x.Location).MaximumLength(500).NotEmpty().When(x => x.DeliveryMode is DeliveryMode.Offline or DeliveryMode.Hybrid);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.MeetingUrl).Must(x => x is null || SharingRules.HttpUrl(x));
        RuleFor(x => x.MeetingUrl).NotEmpty().When(x => x.DeliveryMode is DeliveryMode.Online or DeliveryMode.Hybrid);
        RuleFor(x => new PresentersRequest(x.Presenters)).SetValidator(new PresentersRequestValidator());
        RuleFor(x => new ScheduleContentsRequest(x.ContentIds)).SetValidator(new ScheduleContentsRequestValidator());
        RuleFor(x => new AudienceRequest(x.AudienceScope, x.GenerationIds, x.DepartmentIds)).SetValidator(new AudienceRequestValidator());
    }
}
public sealed class ContentQueryValidator : AbstractValidator<ContentQuery>
{
    public ContentQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000); RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200); RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Sort).Must(x => x is "newest" or "oldest" or "title");
    }
}
public sealed class ScheduleQueryValidator : AbstractValidator<ScheduleQuery>
{
    public ScheduleQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000); RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum(); RuleFor(x => x.DeliveryMode).IsInEnum(); RuleFor(x => x.SharingType).IsInEnum();
        RuleFor(x => x.To).GreaterThan(x => x.From).When(x => x.From.HasValue && x.To.HasValue);
    }
}
