"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  assignMemberGenerationApi,
  addMemberDepartmentApi,
  updateMemberDepartmentApi,
  replaceMemberRolesApi,
  endDepartmentMembershipApi,
  endClubMembershipApi,
} from "../api/member-management.api";
import { memberManagementKeys } from "../queries/member-management.keys";
import { profileKeys } from "@/features/profile/queries/profile.keys";
import type {
  AssignMemberGenRequest,
  AddMemberDepartmentRequest,
  UpdateMemberDepartmentRequest,
  ReplaceMemberRolesRequest,
} from "../types/member-management.types";
import type { ApiError } from "@/core/http/api-error";

export function useAssignMemberGenerationMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, AssignMemberGenRequest>({
    mutationFn: (request) => assignMemberGenerationApi(userId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useAddMemberDepartmentMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    void,
    ApiError,
    { clubMembershipId: string; request: AddMemberDepartmentRequest }
  >({
    mutationFn: ({ clubMembershipId, request }) =>
      addMemberDepartmentApi(userId, clubMembershipId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useUpdateMemberDepartmentMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    void,
    ApiError,
    { departmentMembershipId: string; request: UpdateMemberDepartmentRequest }
  >({
    mutationFn: ({ departmentMembershipId, request }) =>
      updateMemberDepartmentApi(userId, departmentMembershipId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useReplaceMemberRolesMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    void,
    ApiError,
    { departmentMembershipId: string; request: ReplaceMemberRolesRequest }
  >({
    mutationFn: ({ departmentMembershipId, request }) =>
      replaceMemberRolesApi(userId, departmentMembershipId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useEndDepartmentMembershipMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, string>({
    mutationFn: (departmentMembershipId) =>
      endDepartmentMembershipApi(userId, departmentMembershipId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}

export function useEndClubMembershipMutation(userId: string) {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, string>({
    mutationFn: (clubMembershipId) =>
      endClubMembershipApi(userId, clubMembershipId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: memberManagementKeys.memberProfile(userId),
      });
      queryClient.invalidateQueries({ queryKey: profileKeys.all });
    },
  });
}
