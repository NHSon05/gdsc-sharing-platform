using System.Data;
using System.Data.Common;
using FluentValidation;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class RoadmapOperations(ApplicationDbContext db, ICurrentUserService user, IServiceProvider services, ILogger<RoadmapOperations> logger)
{
    public ApplicationDbContext Db => db;
    public bool IsAdmin => user.Roles.Contains(RoleNames.Admin);
    public Guid UserId => user.UserId ?? throw new AuthenticationException("Authentication is required.");

    public void RequireReader()
    {
        if (!user.IsAuthenticated) throw new AuthenticationException("Authentication is required.");
        if (!IsAdmin && !user.Roles.Contains(RoleNames.Member)) throw new ForbiddenAccessException();
    }
    public void RequireAdmin()
    {
        RequireReader();
        if (!IsAdmin) throw new ForbiddenAccessException();
    }
    public Task ValidateAsync<T>(T request, CancellationToken ct) =>
        services.GetRequiredService<IValidator<T>>().ValidateAndThrowAsync(request, ct);

    // Serializable isolation protects predicate checks (cycles, uniqueness, complete reorders)
    // from concurrent writers. A serialization loser receives 409 and can reload/retry.
    public async Task<T> WriteAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        RequireAdmin();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        try
        {
            var result = await action();
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception exception)
        {
            // PostgreSQL may already have rolled back a transaction rejected at commit.
            if (transaction?.GetDbTransaction().Connection is not null)
            {
                try { await transaction.RollbackAsync(CancellationToken.None); }
                catch (Exception rollbackException) when (rollbackException is DbException or InvalidOperationException)
                {
                    logger.LogWarning(rollbackException, "Rollback could not complete; preserving the original write failure.");
                }
            }
            db.ChangeTracker.Clear();
            var postgres = exception.GetBaseException() as PostgresException;
            if (postgres?.SqlState == PostgresErrorCodes.UniqueViolation)
                throw new ConflictException("A record with this slug or edge already exists.");
            if (postgres?.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected
                || exception is DbUpdateConcurrencyException)
                throw new ConflictException("The roadmap changed concurrently. Reload and retry.");
            if (postgres?.SqlState == PostgresErrorCodes.ForeignKeyViolation)
                throw new ConflictException("Related data changed or is still in use. Reload and retry.");
            if (postgres?.SqlState == PostgresErrorCodes.CheckViolation || exception is ArgumentException)
                throw new ApplicationValidationException("The submitted roadmap data is invalid.");
            throw;
        }
    }

    public async Task<Roadmap> RoadmapAsync(Guid id, CancellationToken ct)
        => await db.Roadmaps.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Roadmap", id);

    public async Task<RoadmapNode> NodeAsync(Guid roadmapId, Guid nodeId, CancellationToken ct)
        => await db.RoadmapNodes.SingleOrDefaultAsync(x => x.RoadmapId == roadmapId && x.Id == nodeId, ct)
            ?? throw new NotFoundException("Roadmap node", nodeId);

    public async Task<RoadmapNode> ResourceNodeAsync(Guid nodeId, bool adding, CancellationToken ct)
    {
        var node = await db.RoadmapNodes.Include(x => x.Roadmap).SingleOrDefaultAsync(x => x.Id == nodeId, ct)
            ?? throw new NotFoundException("Roadmap node", nodeId);
        if (adding) RoadmapRules.EnsureCanAdd(node.Roadmap.Status);
        return node;
    }

    public async Task EnsureActiveCategoryAsync(Guid id, CancellationToken ct)
    {
        if (!await db.RoadmapCategories.AnyAsync(x => x.Id == id && x.IsActive, ct))
            throw new ApplicationValidationException("categoryId", "Choose an active roadmap category.");
    }

    public async Task<IReadOnlyList<NodeResponse>> NodesAsync(Guid roadmapId, bool admin, CancellationToken ct) =>
        await db.RoadmapNodes.AsNoTracking().Where(x => x.RoadmapId == roadmapId && (admin || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new NodeResponse(x.Id, x.Title, x.Slug, x.Description, x.NodeType,
                new NodePosition(x.PositionX, x.PositionY), x.Width, x.Color, x.Icon, x.SortOrder, x.IsActive,
                x.Resources.Count(r => admin || r.IsActive))).ToListAsync(ct);

    public async Task<IReadOnlyList<EdgeResponse>> EdgesAsync(Guid roadmapId, bool admin, CancellationToken ct) =>
        await db.RoadmapEdges.AsNoTracking().Where(x => x.RoadmapId == roadmapId
            && (admin || (x.IsActive && x.SourceNode.IsActive && x.TargetNode.IsActive)))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new EdgeResponse(x.Id, x.SourceNodeId, x.TargetNodeId, x.RelationType,
                x.LineStyle, x.Label, x.SortOrder, x.IsActive)).ToListAsync(ct);
}
