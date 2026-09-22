import type { CurrentUserDto } from "@/core/session/session.types";
export type {
  CurrentUserDto,
  DepartmentDto,
} from "@/core/session/session.types";
export type UserProfile = CurrentUserDto;
export interface LoginRequest {
  email: string;
  password: string;
}
export interface AuthResponse {
  user: CurrentUserDto;
}
