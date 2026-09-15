"use client";

import { useQuery } from "@tanstack/react-query";
import { getRoadmapsApi } from "../api/roadmap.api";
import { roadmapKeys } from "../queries/roadmap.keys";
import type {
  PageResponse,
  RoadmapSummary,
  RoadmapQueryParams,
} from "../types/roadmap.types";
import type { ApiError } from "@/core/http/api-error";

export function useRoadmapsQuery(params?: RoadmapQueryParams) {
  return useQuery<PageResponse<RoadmapSummary>, ApiError>({
    queryKey: roadmapKeys.list(params),
    queryFn: ({ signal }) => getRoadmapsApi(params, signal),
    staleTime: 1000 * 60 * 2,
  });
}
