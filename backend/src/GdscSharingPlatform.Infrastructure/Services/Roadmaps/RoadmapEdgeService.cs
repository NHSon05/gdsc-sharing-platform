using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Mapping;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class RoadmapEdgeService(RoadmapOperations op) : IRoadmapEdgeService
{
    public async Task<IReadOnlyList<EdgeResponse>> ListAsync(Guid roadmapId, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.RoadmapAsync(roadmapId, ct);
        return await op.EdgesAsync(roadmapId, true, ct);
    }
    public async Task<EdgeResponse> CreateAsync(Guid roadmapId, EdgeRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            RoadmapRules.EnsureCanAdd((await op.RoadmapAsync(roadmapId, ct)).Status);
            await ValidateGraphAsync(roadmapId, null, request, ct);
            var edge = new RoadmapEdge(roadmapId, request.SourceNodeId, request.TargetNodeId,
                request.RelationType, request.LineStyle, request.SortOrder) { Label = request.Label };
            op.Db.Add(edge);
            return edge.ToResponse();
        }, ct);
    }
    public async Task<EdgeResponse> UpdateAsync(Guid roadmapId, Guid edgeId, EdgeRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var edge = await FindAsync(roadmapId, edgeId, ct);
            await ValidateGraphAsync(roadmapId, edgeId, request, ct);
            edge.Update(request.SourceNodeId, request.TargetNodeId, request.RelationType, request.LineStyle, request.SortOrder);
            edge.Label = request.Label;
            return edge.ToResponse();
        }, ct);
    }
    public Task DeleteAsync(Guid roadmapId, Guid edgeId, CancellationToken ct = default) =>
        op.WriteAsync(async () => { op.Db.RoadmapEdges.Remove(await FindAsync(roadmapId, edgeId, ct)); return true; }, ct);

    private async Task<RoadmapEdge> FindAsync(Guid roadmapId, Guid id, CancellationToken ct) =>
        await op.Db.RoadmapEdges.SingleOrDefaultAsync(x => x.RoadmapId == roadmapId && x.Id == id, ct)
            ?? throw new NotFoundException("Roadmap edge", id);

    private async Task ValidateGraphAsync(Guid roadmapId, Guid? edgeId, EdgeRequest request, CancellationToken ct)
    {
        var count = await op.Db.RoadmapNodes.CountAsync(x => x.RoadmapId == roadmapId
            && (x.Id == request.SourceNodeId || x.Id == request.TargetNodeId), ct);
        if (count != 2) throw new ApplicationValidationException("nodes", "Both endpoints must belong to this roadmap.");
        var edges = await op.Db.RoadmapEdges.AsNoTracking().Where(x => x.RoadmapId == roadmapId && x.Id != edgeId)
            .Select(x => new { x.SourceNodeId, x.TargetNodeId, x.RelationType }).ToListAsync(ct);
        if (edges.Any(x => x.SourceNodeId == request.SourceNodeId && x.TargetNodeId == request.TargetNodeId && x.RelationType == request.RelationType))
            throw new ConflictException("This edge already exists.");
        // Include inactive nodes/edges so later reactivation cannot introduce a cycle.
        if (request.RelationType == RoadmapRelationType.Required && RoadmapRules.CreatesRequiredCycle(
            edges.Where(x => x.RelationType == RoadmapRelationType.Required).Select(x => (x.SourceNodeId, x.TargetNodeId)),
            request.SourceNodeId, request.TargetNodeId))
            throw new ApplicationValidationException("relationType", "Required edges must not form a cycle.");
    }
}
