import { httpClient } from "@/core/http/http-client";
import type {
  PageResponse,
  CategoryResponse,
  RoadmapSummary,
  RoadmapResponse,
  NodeDetailResponse,
  RoadmapQueryParams,
  RoadmapNodeDto,
  RoadmapEdgeDto,
  LearningResourceDto,
  CreateRoadmapRequest,
  UpdateRoadmapRequest,
  UpdateRoadmapStatusRequest,
  CreateNodeRequest,
  UpdateNodeRequest,
  UpdateNodeStatusRequest,
  UpdateNodePositionsRequest,
  CreateEdgeRequest,
  CreateLinkResourceRequest,
} from "../types/roadmap.types";

/* ==========================================================================
   MEMBER APIS
   ========================================================================== */

export async function getRoadmapsApi(
  params?: RoadmapQueryParams,
  signal?: AbortSignal
): Promise<PageResponse<RoadmapSummary>> {
  const response = await httpClient.get<PageResponse<RoadmapSummary>>(
    "/api/roadmaps",
    {
      params,
      signal,
    }
  );
  return response.data;
}

export async function getRoadmapBySlugApi(
  slug: string,
  signal?: AbortSignal
): Promise<RoadmapResponse> {
  const response = await httpClient.get<RoadmapResponse>(
    `/api/roadmaps/${encodeURIComponent(slug)}`,
    { signal }
  );
  return response.data;
}

export async function getNodeDetailApi(
  roadmapId: string,
  nodeId: string,
  signal?: AbortSignal
): Promise<NodeDetailResponse> {
  const response = await httpClient.get<NodeDetailResponse>(
    `/api/roadmaps/${roadmapId}/nodes/${nodeId}`,
    { signal }
  );
  return response.data;
}

export async function getRoadmapCategoriesApi(
  signal?: AbortSignal
): Promise<CategoryResponse[]> {
  const response = await httpClient.get<CategoryResponse[]>(
    "/api/roadmap-categories",
    { signal }
  );
  return response.data;
}

export async function downloadResourceBlobApi(
  resourceId: string,
  signal?: AbortSignal
): Promise<Blob> {
  const response = await httpClient.get<Blob>(
    `/api/roadmap-resources/${resourceId}/download`,
    {
      responseType: "blob",
      signal,
    }
  );
  return response.data;
}

/**
 * Triggers safe browser download of resource attachment using authenticated blob URL.
 */
export async function downloadResourceFile(
  resourceId: string,
  fileName: string
): Promise<void> {
  const blob = await downloadResourceBlobApi(resourceId);
  const blobUrl = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = blobUrl;
  link.setAttribute("download", fileName);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(blobUrl);
}

/* ==========================================================================
   ADMIN APIS
   ========================================================================== */

export async function createRoadmapApi(
  request: CreateRoadmapRequest,
  signal?: AbortSignal
): Promise<RoadmapResponse> {
  const response = await httpClient.post<RoadmapResponse>(
    "/api/admin/roadmaps",
    request,
    { signal }
  );
  return response.data;
}

export async function getAdminRoadmapByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<RoadmapResponse> {
  const response = await httpClient.get<RoadmapResponse>(
    `/api/admin/roadmaps/${id}`,
    { signal }
  );
  return response.data;
}

export async function updateRoadmapApi(
  id: string,
  request: UpdateRoadmapRequest,
  signal?: AbortSignal
): Promise<RoadmapResponse> {
  const response = await httpClient.patch<RoadmapResponse>(
    `/api/admin/roadmaps/${id}`,
    request,
    { signal }
  );
  return response.data;
}

export async function setRoadmapStatusApi(
  id: string,
  request: UpdateRoadmapStatusRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.patch(`/api/admin/roadmaps/${id}/status`, request, {
    signal,
  });
}

export async function createNodeApi(
  roadmapId: string,
  request: CreateNodeRequest,
  signal?: AbortSignal
): Promise<RoadmapNodeDto> {
  const response = await httpClient.post<RoadmapNodeDto>(
    `/api/admin/roadmaps/${roadmapId}/nodes`,
    request,
    { signal }
  );
  return response.data;
}

export async function updateNodeApi(
  roadmapId: string,
  nodeId: string,
  request: UpdateNodeRequest,
  signal?: AbortSignal
): Promise<RoadmapNodeDto> {
  const response = await httpClient.patch<RoadmapNodeDto>(
    `/api/admin/roadmaps/${roadmapId}/nodes/${nodeId}`,
    request,
    { signal }
  );
  return response.data;
}

export async function setNodeStatusApi(
  roadmapId: string,
  nodeId: string,
  request: UpdateNodeStatusRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.patch(
    `/api/admin/roadmaps/${roadmapId}/nodes/${nodeId}/status`,
    request,
    { signal }
  );
}

export async function saveNodePositionsApi(
  roadmapId: string,
  request: UpdateNodePositionsRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.patch(
    `/api/admin/roadmaps/${roadmapId}/nodes/positions`,
    request,
    { signal }
  );
}

export async function createEdgeApi(
  roadmapId: string,
  request: CreateEdgeRequest,
  signal?: AbortSignal
): Promise<RoadmapEdgeDto> {
  const response = await httpClient.post<RoadmapEdgeDto>(
    `/api/admin/roadmaps/${roadmapId}/edges`,
    request,
    { signal }
  );
  return response.data;
}

export async function deleteEdgeApi(
  roadmapId: string,
  edgeId: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(`/api/admin/roadmaps/${roadmapId}/edges/${edgeId}`, {
    signal,
  });
}

export async function createLinkResourceApi(
  nodeId: string,
  request: CreateLinkResourceRequest,
  signal?: AbortSignal
): Promise<LearningResourceDto> {
  const response = await httpClient.post<LearningResourceDto>(
    `/api/admin/roadmap-nodes/${nodeId}/resources/links`,
    request,
    { signal }
  );
  return response.data;
}

export async function uploadFileResourceApi(
  nodeId: string,
  formData: FormData,
  signal?: AbortSignal
): Promise<LearningResourceDto> {
  const response = await httpClient.post<LearningResourceDto>(
    `/api/admin/roadmap-nodes/${nodeId}/resources/files`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
      signal,
    }
  );
  return response.data;
}

export async function deleteResourceApi(
  resourceId: string,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(`/api/admin/roadmap-resources/${resourceId}`, {
    signal,
  });
}
