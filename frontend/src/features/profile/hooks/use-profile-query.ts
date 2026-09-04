"use client";

import { useQuery } from "@tanstack/react-query";
import { getProfileMeApi } from "../api/profile.api";
import { profileKeys } from "../queries/profile.keys";
import type { UserProfileDto } from "../types/profile.types";
import { useSessionStore } from "@/core/session/session.store";
import { selectIsAuthenticated } from "@/core/session/session.selectors";
import type { ApiError } from "@/core/http/api-error";

export function useProfileQuery() {
  const isAuthenticated = useSessionStore(selectIsAuthenticated);

  return useQuery<UserProfileDto, ApiError>({
    queryKey: profileKeys.me(),
    queryFn: ({ signal }) => getProfileMeApi(signal),
    enabled: isAuthenticated,
    staleTime: 1000 * 60 * 5, // 5 minutes cache
  });
}
