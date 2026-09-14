import { useQuery } from "@tanstack/react-query";
import {
  getAdminSchedulesApi,
  getMineSchedulesApi,
  getSchedulesApi,
} from "../api/sharing-schedule.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  ScheduleQueryParams,
  ScheduleResponse,
  SharingPage,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

interface ScheduleQueryOptions {
  refetchInterval?: number | false;
  enabled?: boolean;
}

export function useSchedulesQuery(
  params?: ScheduleQueryParams,
  options?: ScheduleQueryOptions
) {
  return useQuery<SharingPage<ScheduleResponse>, ApiError>({
    queryKey: sharingKeys.schedules.list(params),
    queryFn: ({ signal }) => getSchedulesApi(params, signal),
    refetchInterval: options?.refetchInterval ?? 10_000,
    refetchIntervalInBackground: true,
    refetchOnWindowFocus: true,
    staleTime: 5_000,
    ...options,
  });
}

export function useMineSchedulesQuery(
  params?: ScheduleQueryParams,
  options?: ScheduleQueryOptions
) {
  return useQuery<SharingPage<ScheduleResponse>, ApiError>({
    queryKey: sharingKeys.schedules.mineList(params),
    queryFn: ({ signal }) => getMineSchedulesApi(params, signal),
    refetchInterval: options?.refetchInterval ?? 10_000,
    refetchIntervalInBackground: true,
    refetchOnWindowFocus: true,
    staleTime: 5_000,
    ...options,
  });
}

export function useAdminSchedulesQuery(
  params?: ScheduleQueryParams,
  options?: ScheduleQueryOptions
) {
  return useQuery<SharingPage<ScheduleResponse>, ApiError>({
    queryKey: sharingKeys.schedules.adminList(params),
    queryFn: ({ signal }) => getAdminSchedulesApi(params, signal),
    refetchInterval: options?.refetchInterval ?? 10_000,
    refetchIntervalInBackground: true,
    refetchOnWindowFocus: true,
    staleTime: 5_000,
    ...options,
  });
}
