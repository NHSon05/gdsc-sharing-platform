import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  cancelScheduleApi,
  completeScheduleApi,
  createAdminScheduleApi,
  createScheduleApi,
  deleteScheduleApi,
  publishScheduleApi,
  setScheduleAudienceApi,
  setScheduleContentsApi,
  setSchedulePresentersApi,
  startScheduleApi,
  updateAdminScheduleApi,
  updateScheduleApi,
} from "../api/sharing-schedule.api";
import { sharingKeys } from "../queries/sharing.keys";
import type {
  AudienceRequest,
  CancelScheduleRequest,
  PresentersRequest,
  ScheduleContentsRequest,
  ScheduleRequest,
  ScheduleResponse,
} from "../types/sharing.types";
import type { ApiError } from "@/core/http/api-error";

export function useCreateScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<ScheduleResponse, ApiError, ScheduleRequest>({
    mutationFn: (request) => createScheduleApi(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useUpdateScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: ScheduleRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      updateScheduleApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
      queryClient.setQueryData(sharingKeys.schedules.detail(data.id), data);
    },
  });
}

export function useCreateAdminScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<ScheduleResponse, ApiError, ScheduleRequest>({
    mutationFn: (request) => createAdminScheduleApi(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useUpdateAdminScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: ScheduleRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      updateAdminScheduleApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
      queryClient.setQueryData(sharingKeys.schedules.detail(data.id), data);
    },
  });
}

export function usePublishScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => publishScheduleApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useStartScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => startScheduleApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useCompleteScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; version: number }
  >({
    mutationFn: ({ id, version }) => completeScheduleApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useCancelScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: CancelScheduleRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      cancelScheduleApi(id, request, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}

export function useSetSchedulePresentersMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: PresentersRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      setSchedulePresentersApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
      queryClient.setQueryData(sharingKeys.schedules.detail(data.id), data);
    },
  });
}

export function useSetScheduleContentsMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: ScheduleContentsRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      setScheduleContentsApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
      queryClient.setQueryData(sharingKeys.schedules.detail(data.id), data);
    },
  });
}

export function useSetScheduleAudienceMutation() {
  const queryClient = useQueryClient();

  return useMutation<
    ScheduleResponse,
    ApiError,
    { id: string; request: AudienceRequest; version: number }
  >({
    mutationFn: ({ id, request, version }) =>
      setScheduleAudienceApi(id, request, version),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
      queryClient.setQueryData(sharingKeys.schedules.detail(data.id), data);
    },
  });
}

export function useDeleteScheduleMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, { id: string; version: number }>({
    mutationFn: ({ id, version }) => deleteScheduleApi(id, version),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sharingKeys.schedules.all() });
    },
  });
}
