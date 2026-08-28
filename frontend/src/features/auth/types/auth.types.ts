export interface DepartmentDto {
  id: string;
  name: string;
}

export interface CurrentUserDto {
  id: string;
  email: string;
  displayName: string;
  studentCode?: string;
  generation?: string;
  avatarUrl?: string;
  status: string;
  department?: DepartmentDto;
  roles: string[];
}

export type UserProfile = CurrentUserDto;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
  user: CurrentUserDto;
}

export interface RefreshTokenRequest {
  refreshToken?: string;
}

export interface LogoutRequest {
  refreshToken?: string;
}
