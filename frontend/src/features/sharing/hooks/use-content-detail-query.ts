import { useQuery } from "@tanstack/react-query";
import {
  getAdminContentByIdApi,
  getContentBySlugApi,
  getMineContentByIdApi,
} from "../api/sharing-content.api";
import { sharingKeys } from "../queries/sharing.keys";
import type { ContentResponse } from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useContentBySlugQuery(slug: string, enabled = true) {
  return useQuery<ContentResponse, ApiError>({
    queryKey: sharingKeys.contents.detail(slug),
    queryFn: ({ signal }) => getContentBySlugApi(slug, signal),
    enabled: Boolean(slug) && enabled,
  });
}

export function useMineContentByIdQuery(id: string, enabled = true) {
  return useQuery<ContentResponse, ApiError>({
    queryKey: sharingKeys.contents.mineDetail(id),
    queryFn: ({ signal }) => getMineContentByIdApi(id, signal),
    enabled: Boolean(id) && enabled,
  });
}

export function useAdminContentByIdQuery(id: string, enabled = true) {
  return useQuery<ContentResponse, ApiError>({
    queryKey: sharingKeys.contents.detail(id),
    queryFn: ({ signal }) => getAdminContentByIdApi(id, signal),
    enabled: Boolean(id) && enabled,
  });
}
