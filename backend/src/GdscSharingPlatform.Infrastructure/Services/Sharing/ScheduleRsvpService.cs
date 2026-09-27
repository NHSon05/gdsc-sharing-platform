using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class ScheduleRsvpService(SharingOperations op, SocialOperations social, TimeProvider clock) : IScheduleRsvpService
{
    public async Task<RsvpResponse> UpsertAsync(Guid scheduleId, RsvpRequest request, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        return await social.WriteAsync(async () =>
        {
            await RequireScheduleAsync(scheduleId, true, ct);
            var item = await op.Db.ScheduleRsvps.SingleOrDefaultAsync(x => x.SharingScheduleId == scheduleId && x.UserId == op.UserId, ct);
            var changed = item is null || item.Status != request.Status;
            if (item is null)
            {
                if (request.Version.HasValue) throw new PreconditionFailedException();
                item = new(scheduleId, op.UserId, request.Status);
                op.Db.ScheduleRsvps.Add(item);
            }
            else if (changed)
            {
                if (!request.Version.HasValue) throw new ApplicationValidationException("version", "Supply the current RSVP version.");
                SharingOperations.CheckVersion(item.Version, request.Version.Value);
                item.Update(request.Status);
            }
            // Repeating the desired state is idempotent even when retrying a lost response.
            if (changed) { await op.Db.SaveChangesAsync(ct); await EmitAsync(scheduleId, ct); }
            return Dto(item);
        }, ct);
    }
    public async Task WithdrawAsync(Guid scheduleId, long version, CancellationToken ct) => await social.WriteAsync(async () =>
    {
        await RequireScheduleAsync(scheduleId, true, ct);
        var item = await op.Db.ScheduleRsvps.SingleOrDefaultAsync(x => x.SharingScheduleId == scheduleId && x.UserId == op.UserId, ct);
        if (item is not null)
        {
            SharingOperations.CheckVersion(item.Version, version);
            op.Db.ScheduleRsvps.Remove(item); await op.Db.SaveChangesAsync(ct); await EmitAsync(scheduleId, ct);
        }
        return true;
    }, ct);
    public async Task<RsvpResponse?> GetMineAsync(Guid scheduleId, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await RequireScheduleAsync(scheduleId, false, ct);
        var item = await op.Db.ScheduleRsvps.AsNoTracking().SingleOrDefaultAsync(x => x.SharingScheduleId == scheduleId && x.UserId == op.UserId, ct);
        return item is null ? null : Dto(item);
    }
    public async Task<CursorPage<ScheduleRsvpResponse>> ListForAdminAsync(Guid scheduleId, string? cursor, int pageSize, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await RequireScheduleAsync(scheduleId, false, ct);
        if (pageSize is < 1 or > 100) throw new ApplicationValidationException("pageSize", "Choose 1 to 100 items.");
        var scope = $"rsvps:{scheduleId}";
        var after = SocialCursor.Decode(cursor, scope);
        var source = op.Db.ScheduleRsvps.AsNoTracking().Where(x => x.SharingScheduleId == scheduleId);
        if (after is not null) source = source.Where(x => x.RespondedAtUtc < after.At || x.RespondedAtUtc == after.At && x.Id.CompareTo(after.Id) < 0);
        var rows = await source.OrderByDescending(x => x.RespondedAtUtc).ThenByDescending(x => x.Id).Take(pageSize + 1).ToListAsync(ct);
        var page = rows.Take(pageSize).ToArray();
        op.Audit("ViewRsvps", "Schedule", scheduleId); await op.Db.SaveChangesAsync(ct);
        return new(page.Select(x => new ScheduleRsvpResponse(x.UserId, x.Status, x.RespondedAtUtc, x.UpdatedAtUtc, x.Version)).ToArray(),
            rows.Count > pageSize ? new SocialCursor(page[^1].RespondedAtUtc, page[^1].Id, scope).Encode() : null);
    }
    public async Task<RsvpSummary> GetSummaryAsync(Guid scheduleId, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct);
        var schedule = await RequireScheduleAsync(scheduleId, false, ct);
        var counts = await CountsAsync(scheduleId, ct);
        // Current audience, excluding inactive/deleted users. Presenter/admin exceptions are not the audience denominator.
        var noResponse = await op.Db.Users.CountAsync(u => u.Status == UserStatus.Active && !u.IsDeleted
            && (schedule.AudienceScope == AudienceScope.AllMembers || op.Db.ClubMemberships.Any(m => m.UserId == u.Id && m.IsActive
                && (op.Db.SharingScheduleAudienceGenerations.Any(a => a.SharingScheduleId == scheduleId && a.ClubGenerationId == m.GenerationId)
                    || m.DepartmentMemberships.Any(d => d.IsActive && op.Db.SharingScheduleAudienceDepartments.Any(a => a.SharingScheduleId == scheduleId && a.DepartmentId == d.DepartmentId)))))
            && !op.Db.ScheduleRsvps.Any(r => r.SharingScheduleId == scheduleId && r.UserId == u.Id), ct);
        return new(scheduleId, counts.GetValueOrDefault(ScheduleRsvpStatus.Going), counts.GetValueOrDefault(ScheduleRsvpStatus.Maybe), counts.GetValueOrDefault(ScheduleRsvpStatus.NotGoing), noResponse);
    }
    private async Task<SharingSchedule> RequireScheduleAsync(Guid id, bool mutable, CancellationToken ct)
    {
        var schedule = await op.Db.SharingSchedules.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Schedule", id);
        var uid = op.UserId;
        var presenter = await op.Db.SharingSchedulePresenters.AnyAsync(x => x.SharingScheduleId == id && x.UserId == uid, ct);
        var audience = schedule.Status != SharingScheduleStatus.Draft && (schedule.AudienceScope == AudienceScope.AllMembers
            || await op.Db.ClubMemberships.AnyAsync(m => m.UserId == uid && m.IsActive
                && (op.Db.SharingScheduleAudienceGenerations.Any(a => a.SharingScheduleId == id && a.ClubGenerationId == m.GenerationId)
                    || m.DepartmentMemberships.Any(d => d.IsActive && op.Db.SharingScheduleAudienceDepartments.Any(a => a.SharingScheduleId == id && a.DepartmentId == d.DepartmentId))), ct));
        SocialInteractionAuthorization.RequireScheduleAudience(audience, presenter, op.IsAdmin);
        if (mutable && (schedule.Status != SharingScheduleStatus.Scheduled || schedule.StartsAtUtc <= clock.GetUtcNow()))
            throw new ConflictException("RSVP is closed for this schedule.");
        return schedule;
    }
    private Task<Dictionary<ScheduleRsvpStatus, int>> CountsAsync(Guid id, CancellationToken ct) => op.Db.ScheduleRsvps.Where(x => x.SharingScheduleId == id)
        .GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Status, x => x.Count, ct);
    private async Task EmitAsync(Guid id, CancellationToken ct)
    {
        var counts = await CountsAsync(id, ct);
        social.Emit(SocialEventNames.ScheduleRsvpUpdated, $"schedule:{id}:admins", new
        { scheduleId = id, going = counts.GetValueOrDefault(ScheduleRsvpStatus.Going), maybe = counts.GetValueOrDefault(ScheduleRsvpStatus.Maybe), notGoing = counts.GetValueOrDefault(ScheduleRsvpStatus.NotGoing) });
    }
    private static RsvpResponse Dto(ScheduleRsvp r) => new(r.SharingScheduleId, r.Status, r.UpdatedAtUtc ?? r.RespondedAtUtc, r.Version);
}
