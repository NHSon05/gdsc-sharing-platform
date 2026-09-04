"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createGenerationApi,
  updateGenerationApi,
  deactivateGenerationApi,
} from "../api/member-management.api";
import { memberManagementKeys } from "../queries/member-management.keys";
import { profileKeys } from "@/features/profile/queries/profile.keys";
import type {
  AdminGenerationDto,
  CreateGenerationRequest,
  UpdateGenerationRequest,
} from "../types/member-management.types";
import type { ApiError } from "@/core/http/api-error";

export function useCreateGenerationMutation() {
  const queryClient = useQueryClient();

  return useMutation<AdminGenerationDto, ApiError, CreateGenerationRequest>({
    mutationFn: (request) => createGenerationApi(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useUpdateGenerationMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    AdminGenerationDto,
    ApiError,
    { id: string; request: UpdateGenerationRequest }
  >({
    mutationFn: ({ id, request }) => updateGenerationApi(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useDeactivateGenerationMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, string>({
    mutationFn: (id) => deactivateGenerationApi(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}
