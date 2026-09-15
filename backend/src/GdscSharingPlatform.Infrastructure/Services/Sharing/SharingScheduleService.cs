using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SharingScheduleService(SharingOperations op) : ISharingScheduleService
{
    public async Task<SharingPage<ScheduleResponse>> ListAsync(ScheduleQuery query, bool mine, bool admin, CancellationToken ct)
    {
        if (admin) await op.RequireAdminAsync(ct); else await op.RequireReaderAsync(ct);
        await op.ValidateAsync(query, ct);
        var source = op.VisibleSchedules().AsNoTracking();
        var uid = op.UserId;
        if (mine) source = source.Where(x => x.CreatedByUserId == uid || x.Presenters.Any(p => p.UserId == uid));
        if (query.From.HasValue) { var from = query.From.Value.ToUniversalTime(); source = source.Where(x => x.EndsAtUtc > from); }
        if (query.To.HasValue) { var to = query.To.Value.ToUniversalTime(); source = source.Where(x => x.StartsAtUtc < to); }
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);
        if (query.DeliveryMode.HasValue) source = source.Where(x => x.DeliveryMode == query.DeliveryMode);
        if (query.SharingType.HasValue) source = source.Where(x => x.SharingType == query.SharingType);
        if (query.PresenterId.HasValue) source = source.Where(x => x.Presenters.Any(p => p.UserId == query.PresenterId));
        var count = await source.CountAsync(ct);
        return new(await Project(source.OrderBy(x => x.StartsAtUtc).ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)).ToListAsync(ct), count, query.Page, query.PageSize);
    }

    public async Task<ScheduleResponse> GetAsync(Guid id, bool admin, CancellationToken ct)
    {
        if (admin) await op.RequireAdminAsync(ct); else await op.RequireReaderAsync(ct);
        return await Project(op.VisibleSchedules().AsNoTracking().Where(x => x.Id == id)).SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Schedule", id);
    }

    private IQueryable<ScheduleResponse> Project(IQueryable<SharingSchedule> source)
    {
        var uid = op.UserId;
        var admin = op.IsAdmin;
        return source.Select(x => new ScheduleResponse(x.Id, x.Title, x.Description, x.SharingType, x.DeliveryMode,
            x.StartsAtUtc, x.EndsAtUtc, x.TimeZoneId, x.Location,
            admin || x.Presenters.Any(p => p.UserId == uid)
                || (x.Status != SharingScheduleStatus.Draft && (x.AudienceScope == AudienceScope.AllMembers
                    || op.Db.ClubMemberships.Any(m => m.UserId == uid && m.IsActive
                        && (x.AudienceGenerations.Any(a => a.ClubGenerationId == m.GenerationId)
                            || m.DepartmentMemberships.Any(d => d.IsActive && x.AudienceDepartments.Any(a => a.DepartmentId == d.DepartmentId))))))
                ? x.MeetingUrl : null,
            x.Status, x.AudienceScope, x.CancellationReason, x.CreatedByUserId, x.Version,
            x.Presenters.OrderBy(p => p.SortOrder).ThenBy(p => p.Id).Select(p => new PresenterResponse(p.UserId,
                op.Db.Users.Where(u => u.Id == p.UserId).Select(u => !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName : u.FullName).First(), p.PresenterRole, p.SortOrder)).ToList(),
            x.Contents.Where(c => admin || c.Content.Status == SharingContentStatus.Published
                || c.Content.Authors.Any(a => a.UserId == uid && (a.AuthorRole == SharingAuthorRole.Owner || c.Content.Status != SharingContentStatus.Draft)))
                .OrderBy(c => c.SortOrder).Select(c => new ContentReference(c.SharingContentId, c.Content.Title, c.Content.Slug, c.SortOrder)).ToList(),
            x.AudienceGenerations.Select(a => a.ClubGenerationId).ToList(), x.AudienceDepartments.Select(a => a.DepartmentId).ToList()));
    }

    public async Task<ScheduleResponse> CreateAsync(ScheduleRequest request, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        var start = SharingRules.ToUtc(request.StartsAtLocal, request.TimeZoneId);
        var end = SharingRules.ToUtc(request.EndsAtLocal, request.TimeZoneId, "endsAtLocal");
        var id = await op.WriteAsync(async () =>
        {
            var schedule = new SharingSchedule(request.Title, request.SharingType, request.DeliveryMode, start, end,
                request.TimeZoneId, request.AudienceScope, op.UserId, request.Location, request.MeetingUrl);
            schedule.SetDescription(request.Description);
            op.Db.Add(schedule);
            var presenters = MemberPresenters(request.Presenters);
            await ReplacePresentersAsync(schedule, presenters, ct);
            await ReplaceContentsAsync(schedule, request.ContentIds, ct);
            await ReplaceAudienceAsync(schedule, new(request.AudienceScope, request.GenerationIds, request.DepartmentIds), ct);
            await CheckOverlapAsync(schedule, ct);
            op.Audit("Create", "Schedule", schedule.Id);
            return schedule.Id;
        }, ct);
        return await GetAsync(id, false, ct);
    }

    public async Task<ScheduleResponse> UpdateAsync(Guid id, ScheduleRequest request, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        var start = SharingRules.ToUtc(request.StartsAtLocal, request.TimeZoneId);
        var end = SharingRules.ToUtc(request.EndsAtLocal, request.TimeZoneId, "endsAtLocal");
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct);
            RequireManage(schedule);
            EnsureMutable(schedule);
            // Admin assignment is authoritative, including on schedules originally created by a Member.
            if (!op.IsAdmin)
            {
                var requested = request.Presenters.Count == 0
                    ? new[] { new PresenterRequest(op.UserId, PresenterRole.Speaker) } : request.Presenters;
                if (!schedule.Presenters.Select(p => (p.UserId, p.PresenterRole, p.SortOrder)).ToHashSet()
                    .SetEquals(requested.Select(p => (p.UserId, p.Role, p.SortOrder))))
                    throw new ForbiddenAccessException("Only Admin can change presenter assignments.");
            }
            SharingOperations.Transition(() => schedule.Update(request.Title, request.SharingType, request.DeliveryMode,
                start, end, request.TimeZoneId, request.AudienceScope, op.UserId, request.Location, request.MeetingUrl));
            schedule.SetDescription(request.Description);
            if (op.IsAdmin) await ReplacePresentersAsync(schedule, request.Presenters, ct);
            await ReplaceContentsAsync(schedule, request.ContentIds, ct);
            await ReplaceAudienceAsync(schedule, new(request.AudienceScope, request.GenerationIds, request.DepartmentIds), ct);
            await EnsurePublishableAsync(schedule, ct);
            await CheckOverlapAsync(schedule, ct);
            op.Audit("Update", "Schedule", id, new { schedule.Version, schedule.StartsAtUtc, schedule.EndsAtUtc });
            return true;
        }, ct);
        return await GetAsync(id, false, ct);
    }

    public async Task<ScheduleResponse> SetPresentersAsync(Guid id, PresentersRequest request, long version, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct);
            EnsureMutable(schedule);
            await ReplacePresentersAsync(schedule, request.Presenters, ct);
            await EnsurePublishableAsync(schedule, ct);
            await CheckOverlapAsync(schedule, ct);
            schedule.Touch(op.UserId);
            return true;
        }, ct);
        return await GetAsync(id, true, ct);
    }
    public async Task<ScheduleResponse> SetContentsAsync(Guid id, ScheduleContentsRequest request, long version, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct); EnsureMutable(schedule);
            await ReplaceContentsAsync(schedule, request.ContentIds, ct); schedule.Touch(op.UserId); return true;
        }, ct);
        return await GetAsync(id, true, ct);
    }
    public async Task<ScheduleResponse> SetAudienceAsync(Guid id, AudienceRequest request, long version, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct); EnsureMutable(schedule);
            await ReplaceAudienceAsync(schedule, request, ct); schedule.Touch(op.UserId); return true;
        }, ct);
        return await GetAsync(id, true, ct);
    }
    public async Task<ScheduleResponse> TransitionAsync(Guid id, ScheduleAction action, long version, string? reason, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct);
        if (action == ScheduleAction.Cancel) await op.ValidateAsync(new CancelScheduleRequest(reason!), ct);
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct);
            if (action is ScheduleAction.Publish or ScheduleAction.Start)
            {
                await EnsurePublishableAsync(schedule, ct, true);
                await CheckOverlapAsync(schedule, ct);
            }
            SharingOperations.Transition(() =>
            {
                switch (action)
                {
                    case ScheduleAction.Publish: schedule.Publish(op.UserId); break;
                    case ScheduleAction.Start: schedule.Start(op.UserId); break;
                    case ScheduleAction.Complete: schedule.Complete(op.UserId); break;
                    case ScheduleAction.Cancel: schedule.Cancel(reason!, op.UserId); break;
                    default: throw new ApplicationValidationException("Unknown schedule action.");
                }
            });
            op.Audit(action.ToString(), "Schedule", id, new { schedule.Version });
            return true;
        }, ct);
        return await GetAsync(id, true, ct);
    }
    public async Task DeleteAsync(Guid id, long version, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct);
        await op.WriteAsync(async () =>
        {
            var schedule = await FindAsync(id, version, ct);
            if (schedule.Status != SharingScheduleStatus.Draft) throw new ApplicationValidationException("Only Draft schedules can be deleted.");
            op.Db.RemoveRange(schedule.Presenters); op.Db.RemoveRange(schedule.Contents);
            op.Db.RemoveRange(schedule.AudienceGenerations); op.Db.RemoveRange(schedule.AudienceDepartments);
            op.Db.Remove(schedule); op.Audit("Delete", "Schedule", id); return true;
        }, ct);
    }
    private async Task<SharingSchedule> FindAsync(Guid id, long version, CancellationToken ct)
    {
        var schedule = await op.Db.SharingSchedules.Include(x => x.Presenters).Include(x => x.Contents)
            .Include(x => x.AudienceGenerations).Include(x => x.AudienceDepartments).AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Schedule", id);
        RequireManage(schedule);
        SharingOperations.CheckVersion(schedule.Version, version);
        return schedule;
    }
    private void RequireManage(SharingSchedule schedule)
    { if (!op.IsAdmin && schedule.CreatedByUserId != op.UserId) throw new ForbiddenAccessException(); }
    private static void EnsureMutable(SharingSchedule schedule)
    {
        if (schedule.Status is SharingScheduleStatus.Completed or SharingScheduleStatus.Cancelled)
            throw new ApplicationValidationException("Completed and Cancelled schedules retain their history and cannot be edited.");
    }
    private IReadOnlyList<PresenterRequest> MemberPresenters(IReadOnlyList<PresenterRequest> requested)
    {
        if (op.IsAdmin) return requested;
        if (requested.Any(x => x.UserId != op.UserId || x.Role != PresenterRole.Speaker || x.SortOrder != 0) || requested.Count > 1)
            throw new ForbiddenAccessException("Member schedules are assigned to the creator as Speaker.");
        return [new(op.UserId, PresenterRole.Speaker)];
    }
    private async Task ReplacePresentersAsync(SharingSchedule schedule, IReadOnlyList<PresenterRequest> requested, CancellationToken ct)
    {
        var ids = requested.Select(x => x.UserId).Distinct().ToArray();
        if (await op.Db.Users.CountAsync(x => ids.Contains(x.Id) && x.Status == UserStatus.Active && !x.IsDeleted, ct) != ids.Length)
            throw new ApplicationValidationException("presenters", "Choose active presenters.");
        // Preserve existing rows to avoid unique-key insertion/deletion ordering conflicts.
        foreach (var old in schedule.Presenters.Where(p => !requested.Any(r => r.UserId == p.UserId && r.Role == p.PresenterRole)).ToArray())
        { schedule.Presenters.Remove(old); op.Db.Remove(old); }
        foreach (var r in requested)
        {
            var existing = schedule.Presenters.SingleOrDefault(p => p.UserId == r.UserId && p.PresenterRole == r.Role);
            if (existing is null)
            {
                var presenter = new SharingSchedulePresenter(schedule.Id, r.UserId, r.Role, r.SortOrder);
                schedule.Presenters.Add(presenter); op.Db.Add(presenter);
            }
            else existing.Reorder(r.SortOrder);
        }
        op.Audit("SetPresenters", "Schedule", schedule.Id, new { UserIds = ids });
    }
    private async Task ReplaceContentsAsync(SharingSchedule schedule, IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        if (await op.VisibleContents().CountAsync(x => ids.Contains(x.Id), ct) != ids.Count)
            throw new ApplicationValidationException("contentIds", "Choose content you are allowed to read.");
        foreach (var old in schedule.Contents.Where(c => !ids.Contains(c.SharingContentId)).ToArray())
        { schedule.Contents.Remove(old); op.Db.Remove(old); }
        for (var i = 0; i < ids.Count; i++)
        {
            var existing = schedule.Contents.SingleOrDefault(c => c.SharingContentId == ids[i]);
            if (existing is null)
            {
                var link = new SharingScheduleContent(schedule.Id, ids[i], i);
                schedule.Contents.Add(link); op.Db.Add(link);
            }
            else existing.Reorder(i);
        }
        op.Audit("SetContents", "Schedule", schedule.Id, new { ContentIds = ids });
    }
    private async Task ReplaceAudienceAsync(SharingSchedule schedule, AudienceRequest request, CancellationToken ct)
    {
        if (await op.Db.ClubGenerations.CountAsync(x => request.GenerationIds.Contains(x.Id) && x.IsActive, ct) != request.GenerationIds.Count
            || await op.Db.Departments.CountAsync(x => request.DepartmentIds.Contains(x.Id) && x.IsActive && !x.IsDeleted, ct) != request.DepartmentIds.Count)
            throw new ApplicationValidationException("audience", "Choose active generations and departments.");
        foreach (var old in schedule.AudienceGenerations.Where(a => !request.GenerationIds.Contains(a.ClubGenerationId)).ToArray())
        { schedule.AudienceGenerations.Remove(old); op.Db.Remove(old); }
        foreach (var id in request.GenerationIds.Where(id => !schedule.AudienceGenerations.Any(a => a.ClubGenerationId == id)))
        {
            var target = new SharingScheduleAudienceGeneration(schedule.Id, id);
            schedule.AudienceGenerations.Add(target); op.Db.Add(target);
        }
        foreach (var old in schedule.AudienceDepartments.Where(a => !request.DepartmentIds.Contains(a.DepartmentId)).ToArray())
        { schedule.AudienceDepartments.Remove(old); op.Db.Remove(old); }
        foreach (var id in request.DepartmentIds.Where(id => !schedule.AudienceDepartments.Any(a => a.DepartmentId == id)))
        {
            var target = new SharingScheduleAudienceDepartment(schedule.Id, id);
            schedule.AudienceDepartments.Add(target); op.Db.Add(target);
        }
        schedule.SetAudienceScope(request.AudienceScope);
        op.Audit("SetAudience", "Schedule", schedule.Id, new { request.AudienceScope, request.GenerationIds, request.DepartmentIds });
    }
    private async Task EnsurePublishableAsync(SharingSchedule schedule, CancellationToken ct, bool force = false)
    {
        if (!force && schedule.Status is not (SharingScheduleStatus.Scheduled or SharingScheduleStatus.Ongoing)) return;
        var ids = schedule.Presenters.Select(p => p.UserId).Distinct().ToArray();
        if (ids.Length == 0 || !await op.Db.Users.AnyAsync(x => ids.Contains(x.Id) && x.Status == UserStatus.Active && !x.IsDeleted, ct))
            throw new ApplicationValidationException("presenters", "A published schedule needs an active Presenter.");
        if (schedule.AudienceScope == AudienceScope.SelectedAudience && schedule.AudienceGenerations.Count + schedule.AudienceDepartments.Count == 0)
            throw new ApplicationValidationException("audience", "SelectedAudience requires a generation or department.");
    }
    private async Task CheckOverlapAsync(SharingSchedule schedule, CancellationToken ct)
    {
        var ids = schedule.Presenters.Select(p => p.UserId).Distinct().ToArray();
        var conflict = await op.Db.SharingSchedules.AsNoTracking().Where(x => x.Id != schedule.Id
            && (x.Status == SharingScheduleStatus.Scheduled || x.Status == SharingScheduleStatus.Ongoing)
            && x.StartsAtUtc < schedule.EndsAtUtc && x.EndsAtUtc > schedule.StartsAtUtc
            && x.Presenters.Any(p => ids.Contains(p.UserId)))
            .OrderBy(x => x.StartsAtUtc).Select(x => new { x.Id, x.StartsAtUtc, x.EndsAtUtc }).FirstOrDefaultAsync(ct);
        if (conflict is not null)
            throw new ConflictException($"Presenter conflict with schedule {conflict.Id} ({conflict.StartsAtUtc:O} to {conflict.EndsAtUtc:O}).");
    }
}
