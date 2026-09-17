using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GdscSharingPlatform.Application.Features.Interviews;

public sealed class InterviewQuestionQuery
{
    [Range(1, 1000000)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [StringLength(200)] public string? Search { get; set; }
    [RegularExpression("^(basic|intermediate|advanced)$")] public string? Level { get; set; }
    [StringLength(100)] public string? Department { get; set; }
    [StringLength(100)] public string? Topic { get; set; }
}
public sealed record InterviewQuestionSummary(Guid Id, string Question, string Slug, string Level, string Status, bool NeedsReview);
public sealed record InterviewQuestionPage(IReadOnlyList<InterviewQuestionSummary> Items, int Total, int Page, int PageSize);
public sealed record InterviewQuestionDetail(Guid Id, string Status, JsonElement Data);
public interface IInterviewQuestionService
{
    Task<InterviewQuestionPage> ListAsync(InterviewQuestionQuery query, bool admin, CancellationToken ct);
    Task<InterviewQuestionDetail> GetAsync(Guid id, bool admin, CancellationToken ct);
}
