import type {
  GenerationDto,
  DepartmentDto,
} from "@/features/profile/types/profile.types";

export type AdminDepartmentDto = DepartmentDto & {
  isActive: boolean;
};

export interface CreateDepartmentRequest {
  name: string;
  slug: string;
  description?: string | null;
  color?: string | null;
  icon?: string | null;
  sortOrder?: number;
}

export interface UpdateDepartmentRequest {
  name: string;
  slug: string;
  description?: string | null;
  color?: string | null;
  icon?: string | null;
  sortOrder?: number;
}

export type AdminGenerationDto = GenerationDto & {
  isActive: boolean;
};

export interface CreateGenerationRequest {
  number: number;
  startDate?: string | null;
  endDate?: string | null;
}

export interface UpdateGenerationRequest {
  startDate?: string | null;
  endDate?: string | null;
}

export interface AssignMemberGenRequest {
  generationId: string;
  joinedAt?: string | null;
}

export interface AddMemberDepartmentRequest {
  departmentId: string;
  isPrimary: boolean;
  roleIds: string[];
}

export interface UpdateMemberDepartmentRequest {
  isPrimary: boolean;
  isActive: boolean;
}

export interface ReplaceMemberRolesRequest {
  roleIds: string[];
}

export type UserStatusEnum =
  1 | 2 | 3 | 4 | "Active" | "Inactive" | "Banned" | "Warning";

export interface AdminMemberListItemDto {
  id: string;
  email: string;
  fullName: string;
  displayName?: string | null;
  studentCode?: string | null;
  avatarUrl?: string | null;
  status: UserStatusEnum;
  systemRoles: string[];
  departmentNames: string[];
  generationNumbers: number[];
  createdAt: string;
  lastLoginAt?: string | null;
}

export interface AdminMemberPageDto {
  items: AdminMemberListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AdminMemberListQuery {
  search?: string;
  generationId?: string;
  departmentId?: string;
  status?: number | string;
  systemRole?: string;
  page?: number;
  pageSize?: number;
}

export interface UpdateUserStatusRequest {
  status: number;
}

export interface UpdateUserSystemRolesRequest {
  roles: string[];
}
