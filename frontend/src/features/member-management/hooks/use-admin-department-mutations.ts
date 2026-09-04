"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createDepartmentApi,
  updateDepartmentApi,
  deactivateDepartmentApi,
  activateDepartmentApi,
} from "../api/member-management.api";
import { memberManagementKeys } from "../queries/member-management.keys";
import { profileKeys } from "@/features/profile/queries/profile.keys";
import type {
  AdminDepartmentDto,
  CreateDepartmentRequest,
  UpdateDepartmentRequest,
} from "../types/member-management.types";
import type { ApiError } from "@/core/http/api-error";

export function useCreateDepartmentMutation() {
  const queryClient = useQueryClient();

  return useMutation<AdminDepartmentDto, ApiError, CreateDepartmentRequest>({
    mutationFn: (request) => createDepartmentApi(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useUpdateDepartmentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    AdminDepartmentDto,
    ApiError,
    { id: string; request: UpdateDepartmentRequest }
  >({
    mutationFn: ({ id, request }) => updateDepartmentApi(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useDeactivateDepartmentMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, string>({
    mutationFn: (id) => deactivateDepartmentApi(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useActivateDepartmentMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, string>({
    mutationFn: (id) => activateDepartmentApi(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: memberManagementKeys.all });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}
