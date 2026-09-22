import axios, { type InternalAxiosRequestConfig } from "axios";
import { normalizeAxiosError } from "./api-error";
import { coordinateRefreshToken } from "./refresh-coordinator";
import { useSessionStore } from "@/core/session/session.store";

interface RetryConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
  _sessionRevision?: number;
}
export const httpClient = axios.create({
  baseURL: "",
  timeout: 20000,
  headers: { Accept: "application/json" },
});
httpClient.interceptors.request.use((config: RetryConfig) => {
  config._sessionRevision ??= useSessionStore.getState().revision;
  return config;
});
httpClient.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const original = error.config as RetryConfig | undefined;
      // A request from a previous session must not refresh or clear the new one.
      if (original?._sessionRevision !== useSessionStore.getState().revision)
        throw normalizeAxiosError(error);
      if (original && !original._retry) {
        original._retry = true;
        try {
          await coordinateRefreshToken();
          if (original._sessionRevision !== useSessionStore.getState().revision)
            throw normalizeAxiosError(error);
          return await httpClient(original);
        } catch (refreshError) {
          throw normalizeAxiosError(refreshError);
        }
      }
      useSessionStore.getState().clearSession();
    }
    throw normalizeAxiosError(error);
  }
);
