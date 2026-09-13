import type { RoadmapQueryParams } from "../types/roadmap.types";

export const roadmapKeys = {
  all: ["roadmaps"] as const,
  lists: () => [...roadmapKeys.all, "list"] as const,
  list: (params?: RoadmapQueryParams) =>
    [...roadmapKeys.lists(), params ?? {}] as const,
  details: () => [...roadmapKeys.all, "detail"] as const,
  detail: (slugOrId: string) => [...roadmapKeys.details(), slugOrId] as const,
  categories: () => [...roadmapKeys.all, "categories"] as const,
  nodes: () => [...roadmapKeys.all, "nodes"] as const,
  nodeDetail: (roadmapId: string, nodeId: string) =>
    [...roadmapKeys.nodes(), roadmapId, nodeId] as const,
};
