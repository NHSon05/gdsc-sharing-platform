import { create } from "zustand";
import type { SessionState } from "./session.types";
import {
  AUTH_COOKIE_NAMES,
  getAuthCookie,
  setAuthCookie,
  removeAuthCookie,
} from "./session.cookies";

const USER_STORAGE_KEY = "gdsc_user_profile";

function loadCachedUser() {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function saveCachedUser(user: unknown) {
  if (typeof window === "undefined") return;
  try {
    if (user) {
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
    } else {
      localStorage.removeItem(USER_STORAGE_KEY);
    }
  } catch {
    // Ignore storage quota / access errors
  }
}

function getInitialSessionState() {
  if (typeof document === "undefined") {
    return {
      accessToken: null,
      refreshToken: null,
      user: null,
      status: "idle" as const,
    };
  }

  const accessToken = getAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
  const refreshToken = getAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);
  const user = accessToken ? loadCachedUser() : null;

  return {
    accessToken,
    refreshToken,
    user,
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

  setUser: (user) => {
    saveCachedUser(user);
    set({ user });
  },

  clearSession: () => {
    // 1. Clear cookies
    removeAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
    removeAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

    // 2. Clear cached user
    saveCachedUser(null);

    // 3. Clear Zustand store in RAM
    set({
      accessToken: null,
      refreshToken: null,
      user: null,
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
    const cachedUser = loadCachedUser();
    useSessionStore.setState((state) => ({
      accessToken,
      refreshToken,
      user: state.user || cachedUser,
      status: "authenticated",
    }));
  }
}
