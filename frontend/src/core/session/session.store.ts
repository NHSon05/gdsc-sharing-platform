import { create } from "zustand";
import type { SessionState } from "./session.types";
import {
  AUTH_COOKIE_NAMES,
  AUTH_COOKIE_MAX_AGE,
  getAuthCookie,
  setAuthCookie,
  removeAuthCookie,
} from "./session.cookies";
import { coordinateRefreshToken } from "../http/refresh-coordinator";

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
    status: accessToken
      ? ("authenticated" as const)
      : refreshToken
      ? ("loading" as const)
      : ("idle" as const),
  };
}

export const useSessionStore = create<SessionState>((set) => ({
  ...getInitialSessionState(),

  setTokens: ({ accessToken, refreshToken }) => {
    // 1. Sync with cookies for Next.js Middleware & SSR
    setAuthCookie(
      AUTH_COOKIE_NAMES.ACCESS_TOKEN,
      accessToken,
      AUTH_COOKIE_MAX_AGE.ACCESS_TOKEN
    );
    if (refreshToken) {
      setAuthCookie(
        AUTH_COOKIE_NAMES.REFRESH_TOKEN,
        refreshToken,
        AUTH_COOKIE_MAX_AGE.REFRESH_TOKEN
      );
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
    setAuthCookie(
      AUTH_COOKIE_NAMES.ACCESS_TOKEN,
      accessToken,
      AUTH_COOKIE_MAX_AGE.ACCESS_TOKEN
    );
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
 * If accessToken is missing from cookies but refreshToken is still present,
 * automatically calls the refresh API to restore the accessToken from refreshToken.
 */
export async function initSessionFromCookies(): Promise<void> {
  if (typeof window === "undefined") return;

  const accessToken = getAuthCookie(AUTH_COOKIE_NAMES.ACCESS_TOKEN);
  const refreshToken = getAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN);

  if (accessToken) {
    const cachedUser = loadCachedUser();
    useSessionStore.setState((state) => ({
      accessToken,
      refreshToken: refreshToken || state.refreshToken,
      user: state.user || cachedUser,
      status: "authenticated",
    }));
  } else if (refreshToken) {
    // accessToken is missing from cookies, but refreshToken is still present!
    // Call API to get new accessToken from refreshToken.
    try {
      useSessionStore.setState({ status: "loading", refreshToken });
      const newAccessToken = await coordinateRefreshToken();
      if (newAccessToken) {
        const cachedUser = loadCachedUser();
        useSessionStore.setState((state) => ({
          accessToken: newAccessToken,
          user: state.user || cachedUser,
          status: "authenticated",
        }));
      }
    } catch (error) {
      console.warn(
        "[initSessionFromCookies] Failed to restore session from refreshToken:",
        error
      );
    }
  } else {
    useSessionStore.setState({ status: "unauthenticated" });
  }
}
