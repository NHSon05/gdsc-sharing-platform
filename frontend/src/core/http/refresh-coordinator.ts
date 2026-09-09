import { publicHttpClient } from "./public-http-client";
import { useSessionStore } from "@/core/session/session.store";
import {
  AUTH_COOKIE_NAMES,
  getAuthCookie,
} from "@/core/session/session.cookies";

interface RefreshResponse {
  accessToken: string;
  refreshToken?: string;
}

let activeRefreshPromise: Promise<string> | null = null;

/**
 * Single-flight refresh token coordinator.
 * Prevents race conditions when multiple concurrent requests receive 401 or need a token.
 * Reads refreshToken from Zustand store or document cookies.
 */
export async function coordinateRefreshToken(): Promise<string> {
  if (activeRefreshPromise) {
    return activeRefreshPromise;
  }

  const store = useSessionStore.getState();
  const refreshToken =
    store.refreshToken ||
    (typeof document !== "undefined"
      ? getAuthCookie(AUTH_COOKIE_NAMES.REFRESH_TOKEN)
      : null);

  if (!refreshToken) {
    store.clearSession();
    throw new Error("No refresh token available in cookies or session store.");
  }

  activeRefreshPromise = (async () => {
    try {
      const response = await publicHttpClient.post<RefreshResponse>(
        "/api/auth/refresh",
        { refreshToken }
      );

      const { accessToken: newAccessToken, refreshToken: newRefreshToken } =
        response.data;

      store.setTokens({
        accessToken: newAccessToken,
        refreshToken: newRefreshToken || refreshToken,
      });

      return newAccessToken;
    } catch (error) {
      store.clearSession();
      throw error;
    } finally {
      activeRefreshPromise = null;
    }
  })();

  return activeRefreshPromise;
}
