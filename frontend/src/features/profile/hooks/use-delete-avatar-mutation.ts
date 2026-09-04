"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteAvatarApi } from "../api/profile.api";
import { profileKeys } from "../queries/profile.keys";
import { authKeys } from "@/features/auth/queries/auth.keys";
import type { ApiError } from "@/core/http/api-error";

export function useDeleteAvatarMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, void>({
    mutationFn: () => deleteAvatarApi(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: profileKeys.me() });
      queryClient.invalidateQueries({ queryKey: authKeys.currentUser() });
    },
  });
}
