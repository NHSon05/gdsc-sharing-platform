"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter, useSearchParams } from "next/navigation";
import { loginApi } from "../api/auth.api";
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
    mutationFn: (request) => loginApi(request),
    onSuccess: (data) => {
      // 1. Store tokens in both Zustand Store (RAM) and Cookies
      setTokens({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      });

      // 2. Populate TanStack Query cache with user profile
      queryClient.setQueryData(authKeys.currentUser(), data.user);

      // 3. Navigate to returnUrl or default route
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
