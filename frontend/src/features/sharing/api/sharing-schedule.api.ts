import { httpClient } from "@/core/http/http-client";
import type {
  AudienceRequest,
  CancelScheduleRequest,
  PresentersRequest,
  ScheduleContentsRequest,
  ScheduleQueryParams,
  ScheduleRequest,
  ScheduleResponse,
  SharingPage,
} from "../types/sharing.types";

function ifMatchHeaders(version: number): Record<string, string> {
  return { "If-Match": `"${version}"` };
}

/* ==========================================================================
   MEMBER SCHEDULE APIS
   ========================================================================== */

export async function getSchedulesApi(
  params?: ScheduleQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ScheduleResponse>> {
  const response = await httpClient.get<SharingPage<ScheduleResponse>>(
    "/api/sharing/schedules",
    { params, signal }
  );
  return response.data;
}

export async function getMineSchedulesApi(
  params?: ScheduleQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ScheduleResponse>> {
  const response = await httpClient.get<SharingPage<ScheduleResponse>>(
    "/api/sharing/schedules/mine",
    { params, signal }
  );
  return response.data;
}

export async function getScheduleByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.get<ScheduleResponse>(
    `/api/sharing/schedules/${id}`,
    { signal }
  );
  return response.data;
}

export async function createScheduleApi(
  request: ScheduleRequest,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    "/api/sharing/schedules",
    request,
    { signal }
  );
  return response.data;
}

export async function updateScheduleApi(
  id: string,
  request: ScheduleRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.patch<ScheduleResponse>(
    `/api/sharing/schedules/${id}`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

/* ==========================================================================
   ADMIN SCHEDULE APIS
   ========================================================================== */

export async function getAdminSchedulesApi(
  params?: ScheduleQueryParams,
  signal?: AbortSignal
): Promise<SharingPage<ScheduleResponse>> {
  const response = await httpClient.get<SharingPage<ScheduleResponse>>(
    "/api/admin/sharing/schedules",
    { params, signal }
  );
  return response.data;
}

export async function getAdminScheduleByIdApi(
  id: string,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.get<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}`,
    { signal }
  );
  return response.data;
}

export async function createAdminScheduleApi(
  request: ScheduleRequest,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    "/api/admin/sharing/schedules",
    request,
    { signal }
  );
  return response.data;
}

export async function updateAdminScheduleApi(
  id: string,
  request: ScheduleRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.patch<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function publishScheduleApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/publish`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function startScheduleApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/start`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function completeScheduleApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/complete`,
    {},
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function cancelScheduleApi(
  id: string,
  request: CancelScheduleRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.post<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/cancel`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function setSchedulePresentersApi(
  id: string,
  request: PresentersRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.patch<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/presenters`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function setScheduleContentsApi(
  id: string,
  request: ScheduleContentsRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.patch<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/contents`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function setScheduleAudienceApi(
  id: string,
  request: AudienceRequest,
  version: number,
  signal?: AbortSignal
): Promise<ScheduleResponse> {
  const response = await httpClient.patch<ScheduleResponse>(
    `/api/admin/sharing/schedules/${id}/audience`,
    request,
    {
      headers: ifMatchHeaders(version),
      signal,
    }
  );
  return response.data;
}

export async function deleteScheduleApi(
  id: string,
  version: number,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.delete(`/api/admin/sharing/schedules/${id}`, {
    headers: ifMatchHeaders(version),
    signal,
  });
}
