"use client";

import { useQuery } from "@tanstack/react-query";
import { getRoadmapBySlugApi, getAdminRoadmapByIdApi } from "../api/roadmap.api";
import { roadmapKeys } from "../queries/roadmap.keys";
import type { RoadmapResponse } from "../types/roadmap.types";
import type { ApiError } from "@/core/http/api-error";

const UUID_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export function useRoadmapDetailQuery(slugOrId: string, isAdmin = false) {
  return useQuery<RoadmapResponse, ApiError>({
    queryKey: roadmapKeys.detail(slugOrId),
    queryFn: async ({ signal }) => {
      const isUuid = UUID_REGEX.test(slugOrId);
      if (isUuid && isAdmin) {
        try {
          return await getAdminRoadmapByIdApi(slugOrId, signal);
        } catch {
          // If admin lookup fails, fallback to by slug
        }
      }
      return await getRoadmapBySlugApi(slugOrId, signal);
    },
    enabled: Boolean(slugOrId),
    staleTime: 1000 * 60 * 2,
  });
}
