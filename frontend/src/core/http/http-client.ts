import axios, {
  type AxiosInstance,
  type InternalAxiosRequestConfig,
} from "axios";
import { ENV } from "@/core/config/env";
import { useSessionStore } from "@/core/session/session.store";
import { normalizeAxiosError } from "./api-error";
import { coordinateRefreshToken } from "./refresh-coordinator";

interface CustomRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

export const httpClient: AxiosInstance = axios.create({
  baseURL: ENV.API_BASE_URL,
  timeout: 15000,
  headers: {
    Accept: "application/json",
  },
});

// Request Interceptor: Attach current access token
httpClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const accessToken = useSessionStore.getState().accessToken;

    if (accessToken && !config.headers.Authorization) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(normalizeAxiosError(error));
  }
);

// Response Interceptor: Catch 401, handle single-flight refresh and retry once
httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as CustomRequestConfig | undefined;

    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true;

      try {
        const newAccessToken = await coordinateRefreshToken();
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return httpClient(originalRequest);
      } catch (refreshError) {
        return Promise.reject(normalizeAxiosError(refreshError));
      }
    }

    return Promise.reject(normalizeAxiosError(error));
  }
);
