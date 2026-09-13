"use client";

import { useQuery } from "@tanstack/react-query";
import { getNodeDetailApi } from "../api/roadmap.api";
import { roadmapKeys } from "../queries/roadmap.keys";
import type { NodeDetailResponse } from "../types/roadmap.types";
import type { ApiError } from "@/core/http/api-error";

export function useNodeDetailQuery(
  roadmapId: string | null | undefined,
  nodeId: string | null | undefined
) {
  return useQuery<NodeDetailResponse, ApiError>({
    queryKey: roadmapKeys.nodeDetail(roadmapId ?? "", nodeId ?? ""),
    queryFn: ({ signal }) =>
      getNodeDetailApi(roadmapId as string, nodeId as string, signal),
    enabled: Boolean(roadmapId && nodeId),
    staleTime: 1000 * 60 * 2,
  });
}
