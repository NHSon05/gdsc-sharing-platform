"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { logoutAllApi } from "../api/auth.api";
import { authKeys } from "../queries/auth.keys";
import { useSessionStore } from "@/core/session/session.store";

export function useLogoutAllMutation() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const clearSession = useSessionStore((state) => state.clearSession);

  return useMutation({
    mutationFn: () => logoutAllApi(),
    onSettled: () => {
      clearSession();
      queryClient.removeQueries({ queryKey: authKeys.all });
      router.push("/login");
      router.refresh();
    },
  });
}
