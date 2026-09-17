namespace GdscSharingPlatform.Domain.Interviews;

public sealed class InterviewQuestion
{
    public Guid Id { get; set; }
    public string Data { get; set; } = "{}";
    public string Question { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string Level { get; private set; } = "";
    public string Access { get; private set; } = "";
    public bool NeedsReview { get; private set; }
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; private set; }
}
