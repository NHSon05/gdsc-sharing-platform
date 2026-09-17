"use client";

import { useQuery } from "@tanstack/react-query";
import { getInterviewQuestionsApi } from "../api/learning.api";
import type { InterviewQuestionQuery } from "../types/learning.types";

export function useInterviewQuestionsQuery(query?: InterviewQuestionQuery) {
  return useQuery({
    queryKey: ["interview-questions", query],
    queryFn: ({ signal }) => getInterviewQuestionsApi(query, signal),
    staleTime: 5 * 60 * 1000,
  });
}
