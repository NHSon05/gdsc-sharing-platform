import { httpClient } from "@/core/http/http-client";
import type {
  InterviewQuestionPage,
  InterviewQuestionQuery,
  InterviewQuestionDetail,
} from "../types/learning.types";

export async function getInterviewQuestionsApi(
  query?: InterviewQuestionQuery,
  signal?: AbortSignal
): Promise<InterviewQuestionPage> {
  try {
    // Primary backend route in ASP.NET Core: /api/v1/interview-questions
    const response = await httpClient.get<InterviewQuestionPage>(
      "/api/v1/interview-questions",
      {
        params: query,
        signal,
      }
    );
    return response.data;
  } catch (err: unknown) {
    const errorWithStatus = err as { response?: { status?: number }; status?: number } | undefined;
    const status = errorWithStatus?.response?.status || errorWithStatus?.status;
    if (status === 404) {
      // Fallback to singular endpoint or unversioned route
      try {
        const fallback = await httpClient.get<InterviewQuestionPage>(
          "/api/v1/interview-question",
          {
            params: query,
            signal,
          }
        );
        return fallback.data;
      } catch {
        const unversionedFallback = await httpClient.get<InterviewQuestionPage>(
          "/api/interview-questions",
          {
            params: query,
            signal,
          }
        );
        return unversionedFallback.data;
      }
    }
    throw err;
  }
}

export async function getInterviewQuestionByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<InterviewQuestionDetail> {
  try {
    // Primary backend route in ASP.NET Core: /api/v1/interview-questions/{id}
    const response = await httpClient.get<InterviewQuestionDetail>(
      `/api/v1/interview-questions/${id}`,
      { signal }
    );
    return response.data;
  } catch (err: unknown) {
    const errorWithStatus = err as { response?: { status?: number }; status?: number } | undefined;
    const status = errorWithStatus?.response?.status || errorWithStatus?.status;
    if (status === 404) {
      // Fallback to singular endpoint
      const fallback = await httpClient.get<InterviewQuestionDetail>(
        `/api/v1/interview-question/${id}`,
        { signal }
      );
      return fallback.data;
    }
    throw err;
  }
}
