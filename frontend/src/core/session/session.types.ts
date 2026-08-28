export type AuthenticationStatus =
  "idle" | "authenticated" | "unauthenticated" | "loading";

export interface SessionState {
  accessToken: string | null;
  refreshToken: string | null;
  status: AuthenticationStatus;
  setTokens: (tokens: {
    accessToken: string;
    refreshToken?: string | null;
  }) => void;
  setAccessToken: (accessToken: string) => void;
  clearSession: () => void;
}
