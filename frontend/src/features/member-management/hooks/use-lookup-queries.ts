"use client";

import { useQuery } from "@tanstack/react-query";
import {
  getGenerationsApi,
  getDepartmentsApi,
  getClubRolesApi,
  getMemberProfileByIdApi,
} from "../api/member-management.api";
import { memberManagementKeys } from "../queries/member-management.keys";
import type {
  GenerationDto,
  DepartmentDto,
  ClubRoleDto,
  UserProfileDto,
} from "@/features/profile/types/profile.types";
import type { ApiError } from "@/core/http/api-error";

export function useGenerationsQuery(includeInactive = false) {
  return useQuery<GenerationDto[], ApiError>({
    queryKey: memberManagementKeys.generations(includeInactive),
    queryFn: ({ signal }) => getGenerationsApi(includeInactive, signal),
    staleTime: 1000 * 60 * 5,
  });
}

export function useDepartmentsQuery(includeInactive = false) {
  return useQuery<DepartmentDto[], ApiError>({
    queryKey: memberManagementKeys.departments(includeInactive),
    queryFn: ({ signal }) => getDepartmentsApi(includeInactive, signal),
    staleTime: 1000 * 60 * 5,
  });
}

export function useClubRolesQuery(includeInactive = false) {
  return useQuery<ClubRoleDto[], ApiError>({
    queryKey: memberManagementKeys.clubRoles(includeInactive),
    queryFn: ({ signal }) => getClubRolesApi(includeInactive, signal),
    staleTime: 1000 * 60 * 10,
  });
}

export function useMemberProfileQuery(userId: string) {
  return useQuery<UserProfileDto, ApiError>({
    queryKey: memberManagementKeys.memberProfile(userId),
    queryFn: ({ signal }) => getMemberProfileByIdApi(userId, signal),
    enabled: Boolean(userId),
    staleTime: 1000 * 60 * 2,
  });
}
