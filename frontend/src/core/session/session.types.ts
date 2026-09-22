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
export type AuthenticationStatus =
  "idle" | "authenticated" | "unauthenticated" | "loading";
export interface SessionState {
  revision: number;
  user: CurrentUserDto | null;
  status: AuthenticationStatus;
  setUser: (user: CurrentUserDto | null) => void;
  clearSession: () => void;
}
