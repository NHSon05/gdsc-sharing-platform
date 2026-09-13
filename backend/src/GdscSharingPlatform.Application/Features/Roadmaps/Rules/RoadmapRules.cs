using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Rules;

public static class RoadmapRules
{
    public static bool IsMemberVisible(RoadmapStatus status) => status is RoadmapStatus.Published or RoadmapStatus.Archived;

    public static void EnsureCanAdd(RoadmapStatus status)
    {
        if (status == RoadmapStatus.Archived)
            throw new ConflictException("Archived roadmaps cannot receive new nodes, edges or resources.");
    }

    public static void EnsureCompleteReorder(IEnumerable<Guid> existingIds, IReadOnlyList<Guid> requestedIds)
    {
        if (requestedIds.Distinct().Count() != requestedIds.Count || !existingIds.ToHashSet().SetEquals(requestedIds))
            throw new ApplicationValidationException("ids", "Supply every ID in this collection exactly once.");
    }

    public static bool CreatesRequiredCycle(IEnumerable<(Guid Source, Guid Target)> edges, Guid source, Guid target)
    {
        var adjacency = edges.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.Select(e => e.Target).ToArray());
        var pending = new Stack<Guid>();
        var visited = new HashSet<Guid>();
        pending.Push(target);
        while (pending.TryPop(out var node))
        {
            if (node == source) return true;
            if (!visited.Add(node)) continue;
            if (adjacency.TryGetValue(node, out var next))
                foreach (var id in next) pending.Push(id);
        }
        return false;
    }
}
