"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { uploadAvatarApi } from "../api/profile.api";
import { profileKeys } from "../queries/profile.keys";
import { authKeys } from "@/features/auth/queries/auth.keys";
import type { UploadAvatarResponse } from "../types/profile.types";
import type { ApiError } from "@/core/http/api-error";

export function useUploadAvatarMutation() {
  const queryClient = useQueryClient();

  return useMutation<UploadAvatarResponse, ApiError, File>({
    mutationFn: (file) => uploadAvatarApi(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: profileKeys.me() });
      queryClient.invalidateQueries({ queryKey: authKeys.currentUser() });
    },
  });
}
