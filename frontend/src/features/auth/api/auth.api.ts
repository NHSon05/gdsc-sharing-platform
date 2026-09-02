import { publicHttpClient } from "@/core/http/public-http-client";
import { httpClient } from "@/core/http/http-client";
import type {
  LoginRequest,
  AuthResponse,
  UserProfile,
  LogoutRequest,
} from "../types/auth.types";

/**
 * CLIENT-SIDE API: Sends login request to backend.
 */
export async function loginApi(
  request: LoginRequest,
  signal?: AbortSignal
): Promise<AuthResponse> {
  const response = await publicHttpClient.post<AuthResponse>(
    "/api/auth/login",
    request,
    { signal }
  );
  return response.data;
}

/**
 * CLIENT-SIDE API: Fetches current user profile from backend via Axios httpClient.
 * Automatically attaches Bearer token from Zustand store and handles silent token refresh on 401.
 */
export async function getCurrentUserApi(
  signal?: AbortSignal
): Promise<UserProfile> {
  const response = await httpClient.get<UserProfile>("/api/auth/me", {
    signal,
  });
  return response.data;
}

/**
 * CLIENT-SIDE API: Logs out current session.
 */
export async function logoutApi(
  request?: LogoutRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post("/api/auth/logout", request, { signal });
}

/**
 * CLIENT-SIDE API: Logs out from all active devices/sessions.
 */
export async function logoutAllApi(signal?: AbortSignal): Promise<void> {
  await httpClient.post("/api/auth/logout-all", {}, { signal });
}
