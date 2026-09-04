"use client";

import { useMutation } from "@tanstack/react-query";
import {
  requestEmailChangeApi,
  confirmEmailChangeApi,
} from "../api/profile.api";
import { useSessionStore } from "@/core/session/session.store";
import type {
  EmailChangeRequest,
  EmailConfirmRequest,
} from "../types/profile.types";
import type { ApiError } from "@/core/http/api-error";

export function useRequestEmailChangeMutation() {
  return useMutation<void, ApiError, EmailChangeRequest>({
    mutationFn: (request) => requestEmailChangeApi(request),
  });
}

export function useConfirmEmailChangeMutation() {
  const clearSession = useSessionStore((state) => state.clearSession);

  return useMutation<void, ApiError, EmailConfirmRequest>({
    mutationFn: (request) => confirmEmailChangeApi(request),
    onSuccess: () => {
      // After confirmation, session is revoked on backend, so clear client session
      clearSession();
    },
  });
}
