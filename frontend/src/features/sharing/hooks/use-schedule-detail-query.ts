import { useQuery } from "@tanstack/react-query";
import {
  getAdminScheduleByIdApi,
  getScheduleByIdApi,
} from "../api/sharing-schedule.api";
import { sharingKeys } from "../queries/sharing.keys";
import type { ScheduleResponse } from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useScheduleDetailQuery(id: string, enabled = true) {
  return useQuery<ScheduleResponse, ApiError>({
    queryKey: sharingKeys.schedules.detail(id),
    queryFn: ({ signal }) => getScheduleByIdApi(id, signal),
    enabled: Boolean(id) && enabled,
  });
}

export function useAdminScheduleDetailQuery(id: string, enabled = true) {
  return useQuery<ScheduleResponse, ApiError>({
    queryKey: sharingKeys.schedules.detail(id),
    queryFn: ({ signal }) => getAdminScheduleByIdApi(id, signal),
    enabled: Boolean(id) && enabled,
  });
}
