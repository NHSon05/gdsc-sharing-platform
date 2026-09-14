import { httpClient } from "@/core/http/http-client";
import type {
  ReorderResourcesRequest,
  ResourceMutationResponse,
  ResourceRequest,
  ResourceResponse,
} from "../types/sharing.types";

function ifMatchHeaders(version: number): Record<string, string> {
  return { "If-Match": `"${version}"` };
}

export async function getResourcesApi(
  contentId: string,
  signal?: AbortSignal
): Promise<ResourceResponse[]> {
  const response = await httpClient.get<ResourceResponse[]>(
    `/api/sharing/contents/${contentId}/resources`,
    { signal }
  );
  return response.data;
}

export async function addLinkResourceApi(
  contentId: string,
  request: ResourceRequest,
  version: number,
  signal?: AbortSignal
): Promise<ResourceMutationResponse> {
  const response = await httpClient.post<ResourceMutationResponse>(
    `/api/sharing/contents/${contentId}/resources/links`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function uploadFileResourceApi(
  contentId: string,
  formData: FormData,
  version: number,
  signal?: AbortSignal
): Promise<ResourceMutationResponse> {
  const response = await httpClient.post<ResourceMutationResponse>(
    `/api/sharing/contents/${contentId}/resources/files`,
    formData,
    {
      headers: {
        ...ifMatchHeaders(version),
        "Content-Type": "multipart/form-data",
      },
      signal,
    }
  );
  return response.data;
}

export async function updateResourceApi(
  resourceId: string,
  request: ResourceRequest,
  version: number,
  signal?: AbortSignal
): Promise<ResourceMutationResponse> {
  const response = await httpClient.put<ResourceMutationResponse>(
    `/api/sharing/resources/${resourceId}`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function replaceFileResourceApi(
  resourceId: string,
  file: File,
  version: number,
  signal?: AbortSignal
): Promise<ResourceMutationResponse> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await httpClient.post<ResourceMutationResponse>(
    `/api/sharing/resources/${resourceId}/replace-file`,
    formData,
    {
      headers: {
        ...ifMatchHeaders(version),
        "Content-Type": "multipart/form-data",
      },
      signal,
    }
  );
  return response.data;
}

export async function deleteResourceApi(
  resourceId: string,
  version: number,
  signal?: AbortSignal
): Promise<number> {
  const response = await httpClient.delete<number>(
    `/api/sharing/resources/${resourceId}`,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function reorderResourcesApi(
  contentId: string,
  request: ReorderResourcesRequest,
  version: number,
  signal?: AbortSignal
): Promise<number> {
  const response = await httpClient.put<number>(
    `/api/sharing/contents/${contentId}/resources/reorder`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function downloadSharingResourceBlobApi(
  resourceId: string,
  signal?: AbortSignal
): Promise<Blob> {
  const response = await httpClient.get<Blob>(
    `/api/sharing/resources/${resourceId}/download`,
    {
      responseType: "blob",
      signal,
    }
  );
  return response.data;
}

/**
 * Initiates safe authenticated browser file download.
 */
export async function downloadSharingResourceFile(
  resourceId: string,
  fileName: string
): Promise<void> {
  const blob = await downloadSharingResourceBlobApi(resourceId);
  const blobUrl = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = blobUrl;
  link.setAttribute("download", fileName);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(blobUrl);
}
