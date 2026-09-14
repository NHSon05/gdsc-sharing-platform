import { useQuery, useInfiniteQuery } from "@tanstack/react-query";
import {
  getAdminContentsApi,
  getContentsApi,
  getMineContentsApi,
} from "../api/sharing-content.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  ContentQueryParams,
  ContentSummary,
  SharingPage,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useContentsQuery(params?: ContentQueryParams) {
  return useQuery<SharingPage<ContentSummary>, ApiError>({
    queryKey: sharingKeys.contents.list(params),
    queryFn: ({ signal }) => getContentsApi(params, signal),
  });
}

export function useInfiniteContentsQuery(
  params?: Omit<ContentQueryParams, "page">,
  options?: { pageSize?: number }
) {
  const pageSize = options?.pageSize ?? 5;
  return useInfiniteQuery<SharingPage<ContentSummary>, ApiError>({
    queryKey: sharingKeys.contents.infiniteList({ ...params, pageSize }),
    queryFn: ({ pageParam = 1, signal }) =>
      getContentsApi(
        { ...params, page: pageParam as number, pageSize },
        signal
      ),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => {
      const totalPages = Math.ceil(lastPage.totalCount / lastPage.pageSize);
      return lastPage.page < totalPages ? lastPage.page + 1 : undefined;
    },
  });
}

export function useMineContentsQuery(params?: ContentQueryParams) {
  return useQuery<SharingPage<ContentSummary>, ApiError>({
    queryKey: sharingKeys.contents.mineList(params),
    queryFn: ({ signal }) => getMineContentsApi(params, signal),
  });
}

export function useAdminContentsQuery(params?: ContentQueryParams) {
  return useQuery<SharingPage<ContentSummary>, ApiError>({
    queryKey: sharingKeys.contents.adminList(params),
    queryFn: ({ signal }) => getAdminContentsApi(params, signal),
  });
}
