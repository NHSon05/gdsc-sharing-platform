"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createRoadmapApi,
  updateRoadmapApi,
  setRoadmapStatusApi,
  saveNodePositionsApi,
  createNodeApi,
  updateNodeApi,
  setNodeStatusApi,
  createEdgeApi,
  deleteEdgeApi,
  createLinkResourceApi,
  uploadFileResourceApi,
  deleteResourceApi,
} from "../api/roadmap.api";
import { roadmapKeys } from "../queries/roadmap.keys";
import type {
  CreateRoadmapRequest,
  UpdateRoadmapRequest,
  UpdateRoadmapStatusRequest,
  UpdateNodePositionsRequest,
  CreateNodeRequest,
  UpdateNodeRequest,
  UpdateNodeStatusRequest,
  CreateEdgeRequest,
  CreateLinkResourceRequest,
} from "../types/roadmap.types";
import type { ApiError } from "@/core/http/api-error";

export function useCreateRoadmapMutation() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, CreateRoadmapRequest>({
    mutationFn: (request) => createRoadmapApi(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.lists() });
    },
  });
}

export function useUpdateRoadmapMutation(id: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, UpdateRoadmapRequest>({
    mutationFn: (request) => updateRoadmapApi(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.all });
    },
  });
}

export function useSetRoadmapStatusMutation(id: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, UpdateRoadmapStatusRequest>({
    mutationFn: (request) => setRoadmapStatusApi(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.all });
    },
  });
}

export function useSaveNodePositionsMutation(roadmapId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, UpdateNodePositionsRequest>({
    mutationFn: (request) => saveNodePositionsApi(roadmapId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useCreateNodeMutation(roadmapId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, CreateNodeRequest>({
    mutationFn: (request) => createNodeApi(roadmapId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useUpdateNodeMutation(roadmapId: string, nodeId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, UpdateNodeRequest>({
    mutationFn: (request) => updateNodeApi(roadmapId, nodeId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
      queryClient.invalidateQueries({
        queryKey: roadmapKeys.nodeDetail(roadmapId, nodeId),
      });
    },
  });
}

export function useSetNodeStatusMutation(roadmapId: string, nodeId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, UpdateNodeStatusRequest>({
    mutationFn: (request) => setNodeStatusApi(roadmapId, nodeId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
      queryClient.invalidateQueries({
        queryKey: roadmapKeys.nodeDetail(roadmapId, nodeId),
      });
    },
  });
}

export function useCreateEdgeMutation(roadmapId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, CreateEdgeRequest>({
    mutationFn: (request) => createEdgeApi(roadmapId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useDeleteEdgeMutation(roadmapId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (edgeId) => deleteEdgeApi(roadmapId, edgeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useCreateLinkResourceMutation(
  roadmapId: string,
  nodeId: string
) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, CreateLinkResourceRequest>({
    mutationFn: (request) => createLinkResourceApi(nodeId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: roadmapKeys.nodeDetail(roadmapId, nodeId),
      });
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useUploadFileResourceMutation(
  roadmapId: string,
  nodeId: string
) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, FormData>({
    mutationFn: (formData) => uploadFileResourceApi(nodeId, formData),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: roadmapKeys.nodeDetail(roadmapId, nodeId),
      });
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}

export function useDeleteResourceMutation(roadmapId: string, nodeId: string) {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (resourceId) => deleteResourceApi(resourceId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: roadmapKeys.nodeDetail(roadmapId, nodeId),
      });
      queryClient.invalidateQueries({ queryKey: roadmapKeys.details() });
    },
  });
}
