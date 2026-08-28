import axios, { type AxiosError } from "axios";
import type { ProblemDetails } from "./problem-details";

export class ApiError extends Error {
  public readonly status?: number;
  public readonly title?: string;
  public readonly validationErrors?: Record<string, string[]>;
  public readonly traceId?: string;
  public readonly cause?: unknown;

  constructor(params: {
    status?: number;
    title?: string;
    message: string;
    validationErrors?: Record<string, string[]>;
    traceId?: string;
    cause?: unknown;
  }) {
    super(params.message);
    this.name = "ApiError";
    this.status = params.status;
    this.title = params.title;
    this.validationErrors = params.validationErrors;
    this.traceId = params.traceId;
    this.cause = params.cause;

    Object.setPrototypeOf(this, ApiError.prototype);
  }
}

export function normalizeAxiosError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }

  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ProblemDetails>;
    const response = axiosError.response;

    if (response?.data && typeof response.data === "object") {
      const data = response.data;
      const status = data.status || response.status;
      const title = data.title || axiosError.message;
      const detail =
        data.detail || data.title || "An unexpected error occurred.";
      const validationErrors = data.errors;
      const traceId =
        data.traceId || (data.extensions?.traceId as string | undefined);

      return new ApiError({
        status,
        title,
        message: detail,
        validationErrors,
        traceId,
        cause: error,
      });
    }

    return new ApiError({
      status: response?.status,
      title: axiosError.code || "Network Error",
      message:
        axiosError.message ||
        "Unable to reach the server. Please check your connection.",
      cause: error,
    });
  }

  if (error instanceof Error) {
    return new ApiError({
      title: "Client Error",
      message: error.message,
      cause: error,
    });
  }

  return new ApiError({
    title: "Unknown Error",
    message: "An unexpected error occurred.",
    cause: error,
  });
}
