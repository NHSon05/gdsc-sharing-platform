"use client";

import { useQuery } from "@tanstack/react-query";
import { getInterviewQuestionByIdApi } from "../api/learning.api";

export function useInterviewQuestionDetailQuery(id: string, enabled = true) {
  return useQuery({
    queryKey: ["interview-question-detail", id],
    queryFn: ({ signal }) => getInterviewQuestionByIdApi(id, signal),
    enabled: Boolean(id) && enabled,
    staleTime: 10 * 60 * 1000,
  });
}
