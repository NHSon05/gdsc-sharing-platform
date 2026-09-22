"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { logoutApi } from "../api/auth.api";
import { useSessionStore } from "@/core/session/session.store";

export function useLogoutMutation() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const clearSession = useSessionStore((state) => state.clearSession);

  return useMutation({
    mutationFn: () => logoutApi(),
    onSuccess: async () => {
      // Clear session store in RAM and cookies
      clearSession();
      // Remove cached user data
      await queryClient.cancelQueries();
      queryClient.clear();
      router.push("/login");
      router.refresh();
    },
  });
}
