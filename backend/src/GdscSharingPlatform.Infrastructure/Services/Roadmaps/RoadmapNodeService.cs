using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Mapping;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class RoadmapNodeService(RoadmapOperations op) : IRoadmapNodeService
{
    public async Task<IReadOnlyList<NodeResponse>> ListAsync(Guid roadmapId, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.RoadmapAsync(roadmapId, ct);
        return await op.NodesAsync(roadmapId, true, ct);
    }
    public async Task<NodeDetailResponse> GetAsync(Guid roadmapId, Guid nodeId, CancellationToken ct = default)
    {
        op.RequireReader();
        var roadmap = await op.RoadmapAsync(roadmapId, ct);
        if (!op.IsAdmin && !RoadmapRules.IsMemberVisible(roadmap.Status)) throw new NotFoundException("Node", nodeId);
        var node = await op.Db.RoadmapNodes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == nodeId
            && x.RoadmapId == roadmapId && (op.IsAdmin || x.IsActive), ct) ?? throw new NotFoundException("Node", nodeId);
        var resources = await op.Db.LearningResources.AsNoTracking().Where(x => x.RoadmapNodeId == nodeId && (op.IsAdmin || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        var edges = op.Db.RoadmapEdges.AsNoTracking().Where(x => x.RoadmapId == roadmapId
            && (op.IsAdmin || (x.IsActive && x.SourceNode.IsActive && x.TargetNode.IsActive)));
        var previous = await edges.Where(x => x.TargetNodeId == nodeId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new NodeReference(x.SourceNodeId, x.SourceNode.Title, x.SourceNode.Slug, x.RelationType)).ToListAsync(ct);
        var next = await edges.Where(x => x.SourceNodeId == nodeId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new NodeReference(x.TargetNodeId, x.TargetNode.Title, x.TargetNode.Slug, x.RelationType)).ToListAsync(ct);
        return new(node.ToResponse(resources.Count), node.LearningObjectives, node.EstimatedDuration,
            resources.Select(x => x.ToResponse()).ToArray(), previous, next);
    }
    public async Task<NodeResponse> CreateAsync(Guid roadmapId, NodeRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            RoadmapRules.EnsureCanAdd((await op.RoadmapAsync(roadmapId, ct)).Status);
            var node = new RoadmapNode(roadmapId, request.Title, request.Slug, request.NodeType,
                request.PositionX, request.PositionY, request.Width, request.SortOrder);
            await EnsureUniqueAsync(node, ct);
            Apply(node, request);
            op.Db.Add(node);
            return node.ToResponse(0);
        }, ct);
    }
    public async Task<NodeResponse> UpdateAsync(Guid roadmapId, Guid nodeId, NodeRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var node = await op.NodeAsync(roadmapId, nodeId, ct);
            node.Update(request.Title, request.Slug, request.NodeType, request.PositionX, request.PositionY, request.Width, request.SortOrder);
            await EnsureUniqueAsync(node, ct);
            Apply(node, request);
            return node.ToResponse(await op.Db.LearningResources.CountAsync(x => x.RoadmapNodeId == nodeId, ct));
        }, ct);
    }
    public async Task SetStatusAsync(Guid roadmapId, Guid nodeId, ActiveStatusRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () => { (await op.NodeAsync(roadmapId, nodeId, ct)).SetActive(request.IsActive!.Value); return true; }, ct);
    }
    public async Task SavePositionsAsync(Guid roadmapId, NodePositionsRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            await op.RoadmapAsync(roadmapId, ct);
            var ids = request.Nodes.Select(x => x.Id).ToArray();
            var nodes = await op.Db.RoadmapNodes.Where(x => x.RoadmapId == roadmapId && ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
            if (nodes.Count != ids.Length) throw new ApplicationValidationException("nodes", "Every node must belong to this roadmap.");
            foreach (var position in request.Nodes)
            {
                var node = nodes[position.Id];
                node.SetPosition(position.PositionX, position.PositionY, node.Width);
            }
            return true;
        }, ct);
    }
    private async Task EnsureUniqueAsync(RoadmapNode node, CancellationToken ct)
    {
        if (await op.Db.RoadmapNodes.AnyAsync(x => x.Id != node.Id && x.RoadmapId == node.RoadmapId && x.Slug == node.Slug, ct))
            throw new ConflictException("A node with this slug already exists in the roadmap.");
    }
    private static void Apply(RoadmapNode node, NodeRequest request)
    {
        node.Description = request.Description;
        node.LearningObjectives = request.LearningObjectives;
        node.EstimatedDuration = request.EstimatedDuration;
        node.Color = request.Color;
        node.Icon = request.Icon;
    }
}
