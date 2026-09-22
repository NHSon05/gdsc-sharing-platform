"use client";
import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { getCurrentUserApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import type { CurrentUserDto } from "../types/auth.types";
import { useSessionStore } from "@/core/session/session.store";
import { ApiError } from "@/core/http/api-error";

export function useCurrentUserQuery(initialUser?: CurrentUserDto | null) {
  const status = useSessionStore((state) => state.status);
  const query = useQuery<CurrentUserDto, ApiError>({
    queryKey: authKeys.currentUser(),
    queryFn: async ({ signal }) => {
      const revision = useSessionStore.getState().revision;
      const user = await getCurrentUserApi(signal);
      if (revision !== useSessionStore.getState().revision)
        throw new ApiError({ message: "Session changed" });
      return user;
    },
    enabled: status !== "unauthenticated",
    initialData: initialUser ?? undefined,
    staleTime: 30000,
    retry: false,
    refetchOnMount: "always",
    refetchOnReconnect: true,
    refetchOnWindowFocus: false,
  });
  useEffect(() => {
    if (query.error?.status === 401 || query.error?.status === 403) {
      useSessionStore.getState().clearSession();
    } else if (query.data && !query.error && status !== "unauthenticated")
      useSessionStore.getState().setUser(query.data);
  }, [query.data, query.error, status]);
  return query;
}
