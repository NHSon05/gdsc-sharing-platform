"use client";

import { useQuery } from "@tanstack/react-query";
import { getRoadmapCategoriesApi } from "../api/roadmap.api";
import { roadmapKeys } from "../queries/roadmap.keys";
import type { CategoryResponse } from "../types/roadmap.types";
import type { ApiError } from "@/core/http/api-error";

export function useRoadmapCategoriesQuery() {
  return useQuery<CategoryResponse[], ApiError>({
    queryKey: roadmapKeys.categories(),
    queryFn: ({ signal }) => getRoadmapCategoriesApi(signal),
    staleTime: 1000 * 60 * 10,
  });
}
