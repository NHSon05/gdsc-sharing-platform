using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using FluentValidation;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SharingOperations(ApplicationDbContext db, ICurrentUserService user,
    IServiceProvider services, IHttpContextAccessor http, ILogger<SharingOperations> logger)
{
    public ApplicationDbContext Db => db;
    public Guid UserId => user.UserId ?? throw new AuthenticationException("Authentication is required.");
    public bool IsAdmin => user.Roles.Contains(RoleNames.Admin);

    public async Task RequireReaderAsync(CancellationToken ct)
    {
        if (!user.IsAuthenticated) throw new AuthenticationException("Authentication is required.");
        if (!IsAdmin && !user.Roles.Contains(RoleNames.Member)) throw new ForbiddenAccessException();
        var id = UserId;
        if (!await db.Users.AnyAsync(x => x.Id == id && !x.IsDeleted && x.Status == UserStatus.Active, ct))
            throw new ForbiddenAccessException("An active account is required.");
    }
    public async Task RequireAdminAsync(CancellationToken ct)
    { await RequireReaderAsync(ct); if (!IsAdmin) throw new ForbiddenAccessException(); }

    public Task ValidateAsync<T>(T request, CancellationToken ct) =>
        services.GetRequiredService<IValidator<T>>().ValidateAndThrowAsync(request, ct);

    public IQueryable<SharingContent> VisibleContents()
    {
        var uid = UserId;
        return db.SharingContents.Where(x => IsAdmin || x.Status == SharingContentStatus.Published
            || x.Authors.Any(a => a.UserId == uid && (a.AuthorRole == SharingAuthorRole.Owner || x.Status != SharingContentStatus.Draft)));
    }
    public IQueryable<SharingSchedule> VisibleSchedules()
    {
        var uid = UserId;
        return db.SharingSchedules.Where(x => IsAdmin || x.CreatedByUserId == uid || x.Presenters.Any(p => p.UserId == uid)
            || (x.Status != SharingScheduleStatus.Draft && (x.AudienceScope == AudienceScope.AllMembers
                || db.ClubMemberships.Any(m => m.UserId == uid && m.IsActive
                    && (x.AudienceGenerations.Any(a => a.ClubGenerationId == m.GenerationId)
                        || m.DepartmentMemberships.Any(d => d.IsActive && x.AudienceDepartments.Any(a => a.DepartmentId == d.DepartmentId)))))));
    }
    public async Task<SharingContent> ContentAsync(Guid id, CancellationToken ct) =>
        await db.SharingContents.Include(x => x.Authors).Include(x => x.Tags).SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("Content", id);

    public async Task<SharingContent> EditableContentAsync(Guid id, long version, CancellationToken ct)
    {
        var content = await ContentAsync(id, ct);
        SharingRules.RequireEdit(content, UserId, IsAdmin);
        CheckVersion(content.Version, version);
        return content;
    }
    public static void CheckVersion(long actual, long expected)
    { if (actual != expected) throw new PreconditionFailedException(); }

    public void RequireOwner(SharingContent content)
    {
        if (!content.Authors.Any(a => a.UserId == UserId && a.AuthorRole == SharingAuthorRole.Owner))
            throw new ForbiddenAccessException("Only the Owner can submit or withdraw content.");
    }
    public void Audit(string action, string entity, Guid id, object? metadata = null)
    {
        db.Add(new SharingAuditEntry
        {
            ActorUserId = UserId, Action = action, Entity = entity, EntityId = id,
            TraceId = http.HttpContext?.TraceIdentifier ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"),
            Metadata = JsonSerializer.Serialize(metadata ?? new { })
        });
    }

    // Serializable transactions also protect presenter-overlap predicate reads across different schedules.
    public async Task<T> WriteAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await RequireReaderAsync(ct);
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        try
        {
            var result = await action();
            await db.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken: ct);
            if (tx is not null) await tx.CommitAsync(ct);
            db.ChangeTracker.AcceptAllChanges();
            return result;
        }
        catch (Exception e)
        {
            var contentVersions = db.ChangeTracker.Entries<SharingContent>().Where(x => x.State == EntityState.Modified)
                .Select(x => (x.Entity.Id, Version: x.OriginalValues.GetValue<long>(nameof(SharingContent.Version)))).ToArray();
            var scheduleVersions = db.ChangeTracker.Entries<SharingSchedule>().Where(x => x.State is EntityState.Modified or EntityState.Deleted)
                .Select(x => (x.Entity.Id, Version: x.OriginalValues.GetValue<long>(nameof(SharingSchedule.Version)))).ToArray();
            if (tx?.GetDbTransaction().Connection is not null)
            {
                try { await tx.RollbackAsync(CancellationToken.None); }
                catch (Exception) { logger.LogWarning("Sharing transaction rollback could not complete."); }
            }
            db.ChangeTracker.Clear();
            var pg = e.GetBaseException() as PostgresException;
            if (e is DbUpdateConcurrencyException)
                throw new PreconditionFailedException();
            if (pg?.SqlState == PostgresErrorCodes.SerializationFailure)
            {
                // End the rolled-back transaction before checking committed versions.
                if (tx is not null) await tx.DisposeAsync();
                foreach (var item in contentVersions)
                    if (!await db.SharingContents.AsNoTracking().AnyAsync(x => x.Id == item.Id && x.Version == item.Version, ct))
                        throw new PreconditionFailedException();
                foreach (var item in scheduleVersions)
                    if (!await db.SharingSchedules.AsNoTracking().AnyAsync(x => x.Id == item.Id && x.Version == item.Version, ct))
                        throw new PreconditionFailedException();
                throw new ConflictException("Another sharing transaction changed related data. Reload before retrying.");
            }
            if (pg?.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation
                or PostgresErrorCodes.RestrictViolation or PostgresErrorCodes.DeadlockDetected)
                throw new ConflictException("Sharing data conflicts with an existing or concurrently changed record.");
            if (pg?.SqlState == PostgresErrorCodes.CheckViolation || e is ArgumentException)
                throw new ApplicationValidationException("Invalid sharing data.");
            throw;
        }
    }

    public static void Transition(Action action)
    {
        try { action(); }
        catch (InvalidOperationException e) { throw new ApplicationValidationException(e.Message); }
    }

    public Expression<Func<SharingContent, ContentSummary>> Summary => x =>
        new ContentSummary(x.Id, x.Title, x.Slug, x.Summary, x.CoverImageUrl, x.Status, x.PublishedAtUtc, x.Version)
        {
            Authors = x.Authors.OrderBy(a => a.SortOrder).ThenBy(a => a.Id)
                .Select(a => new AuthorResponse(a.UserId, db.Users.Where(u => u.Id == a.UserId)
                    .Select(u => !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName : u.FullName).First(), a.AuthorRole, a.SortOrder)).ToList(),
            Tags = x.Tags.OrderBy(t => t.Tag.Name).ThenBy(t => t.SharingTagId)
                .Select(t => new TagResponse(t.Tag.Id, t.Tag.Name, t.Tag.Slug, t.Tag.Color, t.Tag.IsActive)).ToList()
        };
    public static Expression<Func<SharingResource, ResourceResponse>> Resource => x =>
        new(x.Id, x.SharingContentId, x.Title, x.Description, x.ResourceType, x.ExternalUrl,
            x.OriginalFileName, x.FileSize, x.ContentType, x.SortOrder, x.IsActive);
    public static ResourceResponse ResourceDto(SharingResource x) => new(x.Id, x.SharingContentId, x.Title,
        x.Description, x.ResourceType, x.ExternalUrl, x.OriginalFileName, x.FileSize, x.ContentType, x.SortOrder, x.IsActive);
}
