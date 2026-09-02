import { create } from "zustand";
import type { SessionState } from "./session.types";
import {
  AUTH_COOKIE_NAMES,
  getAuthCookie,
  setAuthCookie,
  removeAuthCookie,
} from "./session.cookies";

function getInitialSessionState() {
  if (typeof document === "undefined") {
    return {
      accessToken: null,
      refreshToken: null,
      status: "idle" as const,
    };
  }

  const accessToken = getAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
  const refreshToken = getAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

  return {
    accessToken,
    refreshToken,
    status: accessToken ? ("authenticated" as const) : ("idle" as const),
  };
}

export const useSessionStore = create<SessionState>((set) => ({
  ...getInitialSessionState(),

  setTokens: ({ accessToken, refreshToken }) => {
    // 1. Sync with cookies for Next.js Middleware & SSR
    setAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN, accessToken);
    if (refreshToken) {
      setAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN, refreshToken);
    }

    // 2. Update Zustand store in RAM
    set((state) => ({
      accessToken,
      refreshToken:
        refreshToken !== undefined ? refreshToken : state.refreshToken,
      status: "authenticated",
    }));
  },

  setAccessToken: (accessToken: string) => {
    setAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN, accessToken);
    set({
      accessToken,
      status: "authenticated",
    });
  },

  clearSession: () => {
    // 1. Clear cookies
    removeAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
    removeAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

    // 2. Clear Zustand store in RAM
    set({
      accessToken: null,
      refreshToken: null,
      status: "unauthenticated",
    });
  },
}));

/**
 * Initializes session from cookies on client-side mount (zero hydration mismatch).
 */
export function initSessionFromCookies(): void {
  if (typeof window === "undefined") return;

  const accessToken = getAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
  const refreshToken = getAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

  if (accessToken) {
    useSessionStore.setState({
      accessToken,
      refreshToken,
      status: "authenticated",
    });
  }
}
