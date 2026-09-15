import { httpClient } from "@/core/http/http-client";
import type {
  ContentQueryParams,
  ContentRequest,
  ContentResponse,
  ContentSummary,
  ReviewRequest,
  SharingPage,
} from "../types/sharing.types";

function ifMatchHeaders(version: number): Record<string, string> {
  return { "If-Match": `"${version}"` };
}

/* ==========================================================================
   MEMBER CONTENT APIS
   ========================================================================== */

export async function getContentsApi(
  params?: ContentQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ContentSummary>> {
  const response = await httpClient.get<SharingPage<ContentSummary>>(
    "/api/sharing/contents",
    { params, signal }
  );
  return response.data;
}

export async function getMineContentsApi(
  params?: ContentQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ContentSummary>> {
  const response = await httpClient.get<SharingPage<ContentSummary>>(
    "/api/sharing/contents/mine",
    { params, signal }
  );
  return response.data;
}

export async function getContentBySlugApi(
  slug: string,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.get<ContentResponse>(
    `/api/sharing/contents/${encodeURIComponent(slug)}`,
    { signal }
  );
  return response.data;
}

export async function getMineContentByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.get<ContentResponse>(
    `/api/sharing/contents/mine/${id}`,
    { signal }
  );
  return response.data;
}

export async function createContentApi(
  request: ContentRequest,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    "/api/sharing/contents",
    request,
    { signal }
  );
  return response.data;
}

export async function updateContentApi(
  id: string,
  request: ContentRequest,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.patch<ContentResponse>(
    `/api/sharing/contents/${id}`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function submitContentApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/sharing/contents/${id}/submit`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function withdrawContentApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/sharing/contents/${id}/withdraw`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

/* ==========================================================================
   ADMIN REVIEW CONTENT APIS
   ========================================================================== */

export async function getAdminContentsApi(
  params?: ContentQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ContentSummary>> {
  const response = await httpClient.get<SharingPage<ContentSummary>>(
    "/api/admin/sharing/contents",
    { params, signal }
  );
  return response.data;
}

export async function getAdminContentByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.get<ContentResponse>(
    `/api/admin/sharing/contents/${id}`,
    { signal }
  );
  return response.data;
}

export async function approveContentApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/admin/sharing/contents/${id}/approve`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function rejectContentApi(
  id: string,
  request: ReviewRequest,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/admin/sharing/contents/${id}/reject`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function returnToDraftContentApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/admin/sharing/contents/${id}/return-to-draft`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function archiveContentApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ContentResponse> {
  const response = await httpClient.post<ContentResponse>(
    `/api/admin/sharing/contents/${id}/archive`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}
