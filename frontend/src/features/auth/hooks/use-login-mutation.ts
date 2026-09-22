"use client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter, useSearchParams } from "next/navigation";
import { loginApi, getCurrentUserApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import type { LoginRequest, AuthResponse } from "../types/auth.types";
import { useSessionStore } from "@/core/session/session.store";
import { safeReturnPath } from "@/core/session/safe-return-path";
import type { ApiError } from "@/core/http/api-error";

export function useLoginMutation() {
  const router = useRouter();
  const params = useSearchParams();
  const client = useQueryClient();
  return useMutation<AuthResponse, ApiError, LoginRequest>({
    mutationFn: async (request) => {
      await loginApi(request);
      useSessionStore.getState().clearSession();
      // Start a new revision so responses from the previous account cannot win.
      useSessionStore.setState({ status: "loading" });
      return { user: await getCurrentUserApi() };
    },
    onSuccess: async ({ user }) => {
      await client.cancelQueries();
      client.clear();
      useSessionStore.getState().setUser(user);
      client.setQueryData(authKeys.currentUser(), user);
      router.replace(safeReturnPath(params.get("returnUrl")));
      router.refresh();
    },
  });
}
