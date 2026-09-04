import { publicHttpClient } from "./public-http-client";
import { useSessionStore } from "@/core/session/session.store";

interface RefreshResponse {
  accessToken: string;
  refreshToken?: string;
}

let activeRefreshPromise: Promise<string> | null = null;

/**
 * Single-flight refresh token coordinator.
 * Prevents race conditions when multiple concurrent requests receive 401.
 */
export async function coordinateRefreshToken(): Promise<string> {
  if (activeRefreshPromise) {
    return activeRefreshPromise;
  }

  const { refreshToken, clearSession, setTokens } = useSessionStore.getState();

  activeRefreshPromise = (async () => {
    try {
      const response = await publicHttpClient.post<RefreshResponse>(
        "/api/auth/refresh",
        refreshToken ? { refreshToken } : {}
      );

      const { accessToken: newAccessToken, refreshToken: newRefreshToken } =
        response.data;

      setTokens({
        accessToken: newAccessToken,
        refreshToken: newRefreshToken || refreshToken,
      });

      return newAccessToken;
    } catch (error) {
      clearSession();
      throw error;
    } finally {
      activeRefreshPromise = null;
    }
  })();

  return activeRefreshPromise;
}
