"use client";

import { useQuery } from "@tanstack/react-query";
import { getCurrentUserApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import type { CurrentUserDto } from "../types/auth.types";
import { useSessionStore } from "@/core/session/session.store";
import { selectIsAuthenticated } from "@/core/session/session.selectors";
import type { ApiError } from "@/core/http/api-error";

export function useCurrentUserQuery() {
  const isAuthenticated = useSessionStore(selectIsAuthenticated);

  return useQuery<CurrentUserDto, ApiError>({
    queryKey: authKeys.currentUser(),
    queryFn: ({ signal }) => getCurrentUserApi(signal),
    enabled: isAuthenticated,
  });
}
