"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { logoutApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import { useSessionStore } from "@/core/session/session.store";

export function useLogoutMutation() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const clearSession = useSessionStore((state) => state.clearSession);
  const refreshToken = useSessionStore((state) => state.refreshToken);

  return useMutation({
    mutationFn: () => logoutApi({ refreshToken: refreshToken || undefined }),
    onSettled: () => {
      // Clear session store in RAM and cookies
      clearSession();
      // Remove cached user data
      queryClient.removeQueries({ queryKey: authKeys.all });
      router.push("/login");
      router.refresh();
    },
  });
}
