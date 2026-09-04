"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter, useSearchParams } from "next/navigation";
import { loginApi, getCurrentUserApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import type { LoginRequest, AuthResponse } from "../types/auth.types";
import { useSessionStore } from "@/core/session/session.store";
import type { ApiError } from "@/core/http/api-error";

export function useLoginMutation() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const setTokens = useSessionStore((state) => state.setTokens);

  return useMutation<AuthResponse, ApiError, LoginRequest>({
    mutationFn: async (request) => {
      // 1. Perform login to obtain tokens
      const data = await loginApi(request);

      // 2. Store tokens in Zustand & Cookies so httpClient immediately attaches Bearer token
      setTokens({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      });

      // 3. Immediately call API /api/auth/me
      try {
        const user = await getCurrentUserApi();
        // 4. Assign user to Zustand global state immediately
        useSessionStore.getState().setUser(user);
        return {
          ...data,
          user,
        };
      } catch (err) {
        console.error("Failed to fetch /api/auth/me after login:", err);
        if (data.user) {
          useSessionStore.getState().setUser(data.user);
        }
        return data;
      }
    },
    onSuccess: (data) => {
      // 5. Ensure Zustand store and TanStack Query cache are synchronized
      if (data.user) {
        useSessionStore.getState().setUser(data.user);
        queryClient.setQueryData(authKeys.currentUser(), data.user);
      }

      // 6. Navigate to returnUrl or default route
      const returnUrl = searchParams.get("returnUrl");
      if (returnUrl && returnUrl.startsWith("/")) {
        router.push(returnUrl);
      } else {
        router.push("/");
      }

      router.refresh();
    },
  });
}
