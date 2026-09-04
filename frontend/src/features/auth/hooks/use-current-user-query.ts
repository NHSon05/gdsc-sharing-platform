"use client";

import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { getCurrentUserApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import type { CurrentUserDto } from "../types/auth.types";
import { useSessionStore } from "@/core/session/session.store";
import {
  selectIsAuthenticated,
  selectAccessToken,
  selectCurrentUser,
} from "@/core/session/session.selectors";
import {
  AUTH_COOKIE_NAMES,
  getAuthCookie,
} from "@/core/session/session.cookies";
import type { ApiError } from "@/core/http/api-error";

export function useCurrentUserQuery(initialUser?: CurrentUserDto | null) {
  const isAuthenticated = useSessionStore(selectIsAuthenticated);
  const accessToken = useSessionStore(selectAccessToken);
  const storeUser = useSessionStore(selectCurrentUser);
  const setUser = useSessionStore((state) => state.setUser);

  // Read the access token directly from the browser cookie as a bootstrap path.
  // On F5, Zustand and React Query are recreated before effects run, so relying
  // only on selectIsAuthenticated can leave the query disabled for the first render.
  const effectiveToken =
    accessToken ||
    (typeof document !== "undefined"
      ? getAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN)
      : null);

  const query = useQuery<CurrentUserDto, ApiError>({
    queryKey: authKeys.currentUser(),
    queryFn: async ({ signal }) => {
      const user = await getCurrentUserApi(signal);
      useSessionStore.getState().setUser(user);
      return user;
    },
    // Auto-fetch whenever the session has a token or a server-provided user.
    enabled: Boolean(effectiveToken) || isAuthenticated || Boolean(initialUser),
    // Server data keeps header/sidebar populated immediately, while staleTime +
    // refetchOnMount ensure /api/auth/me is still called again after F5.
    initialData: initialUser ?? undefined,
    // Cached Zustand data is a client-only fallback while the network request runs.
    placeholderData: () => storeUser ?? undefined,
    staleTime: 0,
    refetchOnMount: "always",
    refetchOnReconnect: true,
    refetchOnWindowFocus: false,
  });

  useEffect(() => {
    if (query.data) {
      setUser(query.data);
    }
  }, [query.data, setUser]);

  useEffect(() => {
    if (query.error && query.error.status === 401) {
      useSessionStore.getState().clearSession();
    }
  }, [query.error]);

  return query;
}
