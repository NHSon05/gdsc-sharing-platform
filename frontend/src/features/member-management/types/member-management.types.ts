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

export interface MemberListItemDto {
  id: string;
  displayName: string;
  email: string;
  studentCode?: string | null;
  avatarUrl?: string | null;
  systemRoles?: string;
  status?: string;
}
