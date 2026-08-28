import axios, { type AxiosInstance } from "axios";
import { ENV } from "@/core/config/env";
import { normalizeAxiosError } from "./api-error";

export const publicHttpClient: AxiosInstance = axios.create({
  baseURL: ENV.API_BASE_URL,
  timeout: 15000,
  headers: {
    Accept: "application/json",
  },
});

publicHttpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    return Promise.reject(normalizeAxiosError(error));
  }
);
