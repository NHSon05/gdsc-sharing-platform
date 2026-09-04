import { httpClient } from "@/core/http/http-client";
import type {
  GenerationDto,
  DepartmentDto,
  ClubRoleDto,
  UserProfileDto,
} from "@/features/profile/types/profile.types";
import type {
  AdminDepartmentDto,
  CreateDepartmentRequest,
  UpdateDepartmentRequest,
  AdminGenerationDto,
  CreateGenerationRequest,
  UpdateGenerationRequest,
  AssignMemberGenRequest,
  AddMemberDepartmentRequest,
  UpdateMemberDepartmentRequest,
  ReplaceMemberRolesRequest,
} from "../types/member-management.types";

/* ==========================================================================
   LOOKUP APIS
   ========================================================================== */

export async function getGenerationsApi(
  includeInactive = false,
  signal?: AbortSignal
): Promise<GenerationDto[]> {
  const response = await httpClient.get<GenerationDto[]>("/api/generations", {
    params: { includeInactive },
    signal,
  });
  return response.data;
}

export async function getDepartmentsApi(
  includeInactive = false,
  signal?: AbortSignal
): Promise<DepartmentDto[]> {
  const response = await httpClient.get<DepartmentDto[]>("/api/departments", {
    params: { includeInactive },
    signal,
  });
  return response.data;
}

export async function getClubRolesApi(
  includeInactive = false,
  signal?: AbortSignal
): Promise<ClubRoleDto[]> {
  const response = await httpClient.get<ClubRoleDto[]>("/api/club-roles", {
    params: { includeInactive },
    signal,
  });
  return response.data;
}

/* ==========================================================================
   ADMIN DEPARTMENT APIS
   ========================================================================== */

export async function createDepartmentApi(
  request: CreateDepartmentRequest,
  signal?: AbortSignal
): Promise<AdminDepartmentDto> {
  const response = await httpClient.post<AdminDepartmentDto>(
    "/api/admin/departments",
    request,
    { signal }
  );
  return response.data;
}

export async function updateDepartmentApi(
  id: string,
  request: UpdateDepartmentRequest,
  signal?: AbortSignal
): Promise<AdminDepartmentDto> {
  const response = await httpClient.put<AdminDepartmentDto>(
    `/api/admin/departments/${id}`,
    request,
    { signal }
  );
  return response.data;
}

export async function deactivateDepartmentApi(
  id: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(`/api/admin/departments/${id}`, { signal });
}

export async function activateDepartmentApi(
  id: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post(
    `/api/admin/departments/${id}/activate`,
    {},
    { signal }
  );
}

/* ==========================================================================
   ADMIN GENERATION APIS
   ========================================================================== */

export async function createGenerationApi(
  request: CreateGenerationRequest,
  signal?: AbortSignal
): Promise<AdminGenerationDto> {
  const response = await httpClient.post<AdminGenerationDto>(
    "/api/admin/generations",
    request,
    { signal }
  );
  return response.data;
}

export async function updateGenerationApi(
  id: string,
  request: UpdateGenerationRequest,
  signal?: AbortSignal
): Promise<AdminGenerationDto> {
  const response = await httpClient.put<AdminGenerationDto>(
    `/api/admin/generations/${id}`,
    request,
    { signal }
  );
  return response.data;
}

export async function deactivateGenerationApi(
  id: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(`/api/admin/generations/${id}`, { signal });
}

/* ==========================================================================
   ADMIN MEMBER MEMBERSHIP APIS
   ========================================================================== */

export async function getMemberProfileByIdApi(
  userId: string,
  signal?: AbortSignal
): Promise<UserProfileDto> {
  const response = await httpClient.get<UserProfileDto>(
    `/api/admin/members/${userId}`,
    { signal }
  );
  return response.data;
}

export async function assignMemberGenerationApi(
  userId: string,
  request: AssignMemberGenRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post(`/api/admin/members/${userId}/memberships`, request, {
    signal,
  });
}

export async function addMemberDepartmentApi(
  userId: string,
  clubMembershipId: string,
  request: AddMemberDepartmentRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post(
    `/api/admin/members/${userId}/memberships/${clubMembershipId}/departments`,
    request,
    { signal }
  );
}

export async function updateMemberDepartmentApi(
  userId: string,
  departmentMembershipId: string,
  request: UpdateMemberDepartmentRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.put(
    `/api/admin/members/${userId}/department-memberships/${departmentMembershipId}`,
    request,
    { signal }
  );
}

export async function replaceMemberRolesApi(
  userId: string,
  departmentMembershipId: string,
  request: ReplaceMemberRolesRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.put(
    `/api/admin/members/${userId}/department-memberships/${departmentMembershipId}/roles`,
    request,
    { signal }
  );
}

export async function endDepartmentMembershipApi(
  userId: string,
  departmentMembershipId: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(
    `/api/admin/members/${userId}/department-memberships/${departmentMembershipId}`,
    { signal }
  );
}

export async function endClubMembershipApi(
  userId: string,
  clubMembershipId: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(
    `/api/admin/members/${userId}/memberships/${clubMembershipId}`,
    { signal }
  );
}
