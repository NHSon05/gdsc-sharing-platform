import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getAdminTagsApi,
  getTagsApi,
  saveTagApi,
  setTagStatusApi,
} from "../api/sharing-tag.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  TagRequest,
  TagResponse,
  TagStatusRequest,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useTagsQuery() {
  return useQuery<TagResponse[], ApiError>({
    queryKey: sharingKeys.tags.all(),
    queryFn: ({ signal }) => getTagsApi(signal),
    staleTime: 60 * 1000,
  });
}

export function useAdminTagsQuery() {
  return useQuery<TagResponse[], ApiError>({
    queryKey: sharingKeys.tags.admin(),
    queryFn: ({ signal }) => getAdminTagsApi(signal),
  });
}

export function useSaveTagMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    TagResponse,
    ApiError,
    { request: TagRequest; id?: string }
  >({
    mutationFn: ({ request, id }) => saveTagApi(request, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.tags.all() });
    },
  });
}

export function useSetTagStatusMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    TagResponse,
    ApiError,
    { id: string; request: TagStatusRequest }
  >({
    mutationFn: ({ id, request }) => setTagStatusApi(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.tags.all() });
    },
  });
}
