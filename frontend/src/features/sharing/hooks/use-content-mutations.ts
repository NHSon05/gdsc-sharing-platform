import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  approveContentApi,
  archiveContentApi,
  createContentApi,
  rejectContentApi,
  returnToDraftContentApi,
  submitContentApi,
  updateContentApi,
  withdrawContentApi,
} from "../api/sharing-content.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  ContentRequest,
  ContentResponse,
  ReviewRequest,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useCreateContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<ContentResponse, ApiError, ContentRequest>({
    mutationFn: (request) => createContentApi(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
      queryClient.setQueryData(
        sharingKeys.contents.mineDetail(data.content.id),
        data
      );
    },
  });
}

export function useUpdateContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; request: ContentRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      updateContentApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
      queryClient.setQueryData(
        sharingKeys.contents.mineDetail(data.content.id),
        data
      );
      if (data.content.slug) {
        queryClient.setQueryData(
          sharingKeys.contents.detail(data.content.slug),
          data
        );
      }
    },
  });
}

export function useSubmitContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => submitContentApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}

export function useWithdrawContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => withdrawContentApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}

export function useApproveContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => approveContentApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}

export function useRejectContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; request: ReviewRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      rejectContentApi(id, request, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}

export function useReturnToDraftMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => returnToDraftContentApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}

export function useArchiveContentMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ContentResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => archiveContentApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.contents.all() });
    },
  });
}
