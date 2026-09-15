import { httpClient } from "@/core/http/http-client";
import type {
  TagRequest,
  TagResponse,
  TagStatusRequest,
} from "../types/sharing.types";

export async function getTagsApi(signal?: AbortSignal): Promise<TagResponse[]> {
  const response = await httpClient.get<TagResponse[]>("/api/sharing/tags", {
    signal,
  });
  return response.data;
}

export async function getAdminTagsApi(
  signal?: AbortSignal
): Promise<TagResponse[]> {
  const response = await httpClient.get<TagResponse[]>(
    "/api/admin/sharing/tags",
    { signal }
  );
  return response.data;
}

export async function saveTagApi(
  request: TagRequest,
  id?: string,
  signal?: AbortSignal
): Promise<TagResponse> {
  if (id) {
    const response = await httpClient.put<TagResponse>(
      `/api/admin/sharing/tags/${id}`,
      request,
      { signal }
    );
    return response.data;
  }
  const response = await httpClient.post<TagResponse>(
    "/api/admin/sharing/tags",
    request,
    { signal }
  );
  return response.data;
}

export async function setTagStatusApi(
  id: string,
  request: TagStatusRequest,
  signal?: AbortSignal
): Promise<TagResponse> {
  const response = await httpClient.patch<TagResponse>(
    `/api/admin/sharing/tags/${id}/status`,
    request,
    { signal }
  );
  return response.data;
}
