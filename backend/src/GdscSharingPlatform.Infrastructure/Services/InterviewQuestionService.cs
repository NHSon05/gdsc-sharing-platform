using System.Text.Json;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Interviews;
using GdscSharingPlatform.Domain.Interviews;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class InterviewQuestionService(ApplicationDbContext db, ICurrentUserService user) : IInterviewQuestionService
{
    private IQueryable<InterviewQuestion> Query(bool admin)
    {
        if (!user.IsAuthenticated) throw new AuthenticationException("Authentication is required.");
        var isAdmin = user.Roles.Contains(RoleNames.Admin);
        if ((admin && !isAdmin) || (!isAdmin && !user.Roles.Contains(RoleNames.Member))) throw new ForbiddenAccessException();
        return db.InterviewQuestions.AsNoTracking().Where(q => admin || (q.Status == "published" && !q.NeedsReview));
    }
    public async Task<InterviewQuestionPage> ListAsync(InterviewQuestionQuery query, bool admin, CancellationToken ct)
    {
        Validator(query);
        var rows = Query(admin);
        if (!string.IsNullOrWhiteSpace(query.Search)) rows = rows.Where(q => q.Question.ToLower().Contains(query.Search.Trim().ToLower()));
        if (query.Level is not null) rows = rows.Where(q => q.Level == query.Level);
        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var filter = JsonSerializer.Serialize(new { departments = new[] { query.Department } });
            rows = rows.Where(q => EF.Functions.JsonContains(q.Data, filter));
        }
        if (!string.IsNullOrWhiteSpace(query.Topic))
        {
            var filter = JsonSerializer.Serialize(new { topics = new[] { query.Topic } });
            rows = rows.Where(q => EF.Functions.JsonContains(q.Data, filter));
        }
        var total = await rows.CountAsync(ct);
        var items = await rows.OrderBy(q => q.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(q => new InterviewQuestionSummary(q.Id, q.Question, q.Slug, q.Level, q.Status, q.NeedsReview)).ToListAsync(ct);
        return new(items, total, query.Page, query.PageSize);
    }
    public async Task<InterviewQuestionDetail> GetAsync(Guid id, bool admin, CancellationToken ct)
    {
        var row = await Query(admin).SingleOrDefaultAsync(q => q.Id == id, ct) ?? throw new NotFoundException("InterviewQuestion", id);
        using var document = JsonDocument.Parse(row.Data);
        return new(row.Id, row.Status, document.RootElement.Clone());
    }
    private static void Validator(InterviewQuestionQuery query) => System.ComponentModel.DataAnnotations.Validator.ValidateObject(query, new(query), true);
}
