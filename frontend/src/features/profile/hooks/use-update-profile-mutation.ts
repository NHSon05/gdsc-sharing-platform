"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateProfileMeApi } from "../api/profile.api";
import { profileKeys } from "../queries/profile.keys";
import { authKeys } from "@/features/auth/queries/auth.keys";
import type {
  UpdateProfileRequest,
  UserProfileDto,
} from "../types/profile.types";
import type { ApiError } from "@/core/http/api-error";

export function useUpdateProfileMutation() {
  const queryClient = useQueryClient();

  return useMutation<UserProfileDto, ApiError, UpdateProfileRequest>({
    mutationFn: (request) => updateProfileMeApi(request),
    onSuccess: (data) => {
      queryClient.setQueryData(profileKeys.me(), data);
      queryClient.invalidateQueries({ queryKey: authKeys.currentUser() });
    },
  });
}
