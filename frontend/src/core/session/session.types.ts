import type { CurrentUserDto } from "@/features/auth/types/auth.types";

export type AuthenticationStatus =
  "idle" | "authenticated" | "unauthenticated" | "loading";

export interface SessionState {
  accessToken: string | null;
  refreshToken: string | null;
  user: CurrentUserDto | null;
  status: AuthenticationStatus;
  setTokens: (tokens: {
    accessToken: string;
    refreshToken?: string | null;
  }) => void;
  setAccessToken: (accessToken: string) => void;
  setUser: (user: CurrentUserDto | null) => void;
  clearSession: () => void;
}
