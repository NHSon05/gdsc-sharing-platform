import { publicHttpClient } from "@/core/http/public-http-client";
import { httpClient } from "@/core/http/http-client";
import type {
  LoginRequest,
  AuthResponse,
  UserProfile,
  LogoutRequest,
} from "../types/auth.types";

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

export async function getCurrentUserApi(
  signal?: AbortSignal
): Promise<UserProfile> {
  const response = await httpClient.get<UserProfile>("/api/auth/me", {
    signal,
  });
  return response.data;
}

export async function logoutApi(
  request?: LogoutRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post("/api/auth/logout", request, { signal });
}

export async function logoutAllApi(signal?: AbortSignal): Promise<void> {
  await httpClient.post("/api/auth/logout-all", {}, { signal });
}
