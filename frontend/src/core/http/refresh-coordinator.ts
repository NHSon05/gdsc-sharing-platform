import { publicHttpClient } from "./public-http-client";
import { useSessionStore } from "@/core/session/session.store";
import { ApiError, normalizeAxiosError } from "./api-error";

let activeRefreshPromise: Promise<void> | null = null;
export function coordinateRefreshToken(): Promise<void> {
  if (activeRefreshPromise) return activeRefreshPromise;
  const revision = useSessionStore.getState().revision;
  const refresh = async () => {
    if (useSessionStore.getState().status === "unauthenticated")
      throw new ApiError({ status: 401, message: "Session expired" });
    // A late 401 or another tab may already have refreshed the browser cookies.
    try {
      await publicHttpClient.get("/api/auth/me");
      return;
    } catch (error) {
      if (normalizeAxiosError(error).status !== 401) throw error;
    }
    await publicHttpClient.post("/api/auth/refresh");
  };
  // Web Locks serialize rotations across tabs; the server also coalesces requests.
  const run = async (): Promise<void> => {
    if (typeof navigator !== "undefined" && navigator.locks) {
      await navigator.locks.request("gdsc-session-refresh", refresh);
    } else await refresh();
  };
  activeRefreshPromise = run()
    .catch((error: unknown) => {
      const normalized = normalizeAxiosError(error);
      if (
        revision === useSessionStore.getState().revision &&
        (normalized.status === 401 || normalized.status === 403)
      )
        useSessionStore.getState().clearSession();
      throw normalized;
    })
    .finally(() => {
      activeRefreshPromise = null;
    });
  return activeRefreshPromise;
}
