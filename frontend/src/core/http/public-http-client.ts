import axios, { type AxiosInstance } from "axios";
import { normalizeAxiosError } from "./api-error";

export const publicHttpClient: AxiosInstance = axios.create({
  baseURL: "",
  timeout: 20000,
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
