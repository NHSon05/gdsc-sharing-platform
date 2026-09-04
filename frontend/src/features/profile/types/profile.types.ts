export interface GenerationDto {
  id: string;
  number: number;
  name: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

export interface DepartmentDto {
  id: string;
  name: string;
  slug: string;
  description?: string;
  color?: string;
  icon?: string;
  sortOrder?: number;
  isActive?: boolean;
}

export interface ClubRoleDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface DepartmentMembershipDto {
  id: string;
  department: DepartmentDto;
  isPrimary: boolean;
  isActive?: boolean;
  roles: ClubRoleDto[];
}

export interface ClubMembershipDto {
  id: string;
  generation: GenerationDto;
  isActive: boolean;
  departments: DepartmentMembershipDto[];
}

export interface UserProfileDto {
  id: string;
  displayName: string;
  email: string;
  phoneNumber?: string | null;
  studentCode?: string | null;
  githubUrl?: string | null;
  bio?: string | null;
  avatarUrl?: string | null;
  systemRoles?: string;
  memberships: ClubMembershipDto[];
  profileCompletionPercentage: number;
  missingProfileFields: string[];
  updatedAtUtc?: string;
}

export interface UpdateProfileRequest {
  displayName: string;
  phoneNumber?: string | null;
  studentCode?: string | null;
  githubUrl?: string | null;
  bio?: string | null;
}

export interface UploadAvatarResponse {
  avatarUrl: string;
}

export interface EmailChangeRequest {
  newEmail: string;
  currentPassword: string;
}

export interface EmailConfirmRequest {
  token: string;
  email: string;
}
