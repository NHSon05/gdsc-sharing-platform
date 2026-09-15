import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addLinkResourceApi,
  deleteResourceApi,
  getResourcesApi,
  reorderResourcesApi,
  replaceFileResourceApi,
  updateResourceApi,
  uploadFileResourceApi,
} from "../api/sharing-resource.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  ReorderResourcesRequest,
  ResourceMutationResponse,
  ResourceRequest,
  ResourceResponse,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useResourcesQuery(contentId: string, enabled = true) {
  return useQuery<ResourceResponse[], ApiError>({
    queryKey: sharingKeys.resources.byContent(contentId),
    queryFn: ({ signal }) => getResourcesApi(contentId, signal),
    enabled: Boolean(contentId) && enabled,
  });
}

export function useAddLinkResourceMutation(contentId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    ResourceMutationResponse,
    ApiError,
    { request: ResourceRequest; version: number }
  >({
    mutationFn: ({ request, version }) =>
      addLinkResourceApi(contentId, request, version),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: sharingKeys.resources.byContent(contentId),
      });
      queryClient.invalidateQueries({
        queryKey: sharingKeys.contents.all(),
      });
    },
  });
}

export function useUploadFileResourceMutation(contentId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    ResourceMutationResponse,
    ApiError,
    { formData: FormData; version: number }
  >({
    mutationFn: ({ formData, version }) =>
      uploadFileResourceApi(contentId, formData, version),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: sharingKeys.resources.byContent(contentId),
      });
      queryClient.invalidateQueries({
        queryKey: sharingKeys.contents.all(),
      });
    },
  });
}

export function useUpdateResourceMutation(contentId?: string) {
  const queryClient = useQueryClient();

  return useMutation<
    ResourceMutationResponse,
    ApiError,
    { resourceId: string; request: ResourceRequest; version: number }
  >({
    mutationFn: ({ resourceId, request, version }) =>
      updateResourceApi(resourceId, request, version),
    onSuccess: () => {
      if (contentId) {
        queryClient.invalidateQueries({
          queryKey: sharingKeys.resources.byContent(contentId),
        });
      }
      queryClient.invalidateQueries({
        queryKey: sharingKeys.contents.all(),
      });
    },
  });
}

export function useReplaceFileResourceMutation(contentId?: string) {
  const queryClient = useQueryClient();

  return useMutation<
    ResourceMutationResponse,
    ApiError,
    { resourceId: string; file: File; version: number }
  >({
    mutationFn: ({ resourceId, file, version }) =>
      replaceFileResourceApi(resourceId, file, version),
    onSuccess: () => {
      if (contentId) {
        queryClient.invalidateQueries({
          queryKey: sharingKeys.resources.byContent(contentId),
        });
      }
      queryClient.invalidateQueries({
        queryKey: sharingKeys.contents.all(),
      });
    },
  });
}

export function useDeleteResourceMutation(contentId?: string) {
  const queryClient = useQueryClient();

  return useMutation<number, ApiError, { resourceId: string; version: number }>(
    {
      mutationFn: ({ resourceId, version }) =>
        deleteResourceApi(resourceId, version),
      onSuccess: () => {
        if (contentId) {
          queryClient.invalidateQueries({
            queryKey: sharingKeys.resources.byContent(contentId),
          });
        }
        queryClient.invalidateQueries({
          queryKey: sharingKeys.contents.all(),
        });
      },
    }
  );
}

export function useReorderResourcesMutation(contentId: string) {
  const queryClient = useQueryClient();

  return useMutation<
    number,
    ApiError,
    { request: ReorderResourcesRequest; version: number }
  >({
    mutationFn: ({ request, version }) =>
      reorderResourcesApi(contentId, request, version),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: sharingKeys.resources.byContent(contentId),
      });
      queryClient.invalidateQueries({
        queryKey: sharingKeys.contents.all(),
      });
    },
  });
}
