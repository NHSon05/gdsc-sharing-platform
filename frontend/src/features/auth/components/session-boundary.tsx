"use client";
import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useSessionStore } from "@/core/session/session.store";

/** Session loss from ANY API request invalidates private cache, not only /me. */
export function SessionBoundary({ children }: { children: React.ReactNode }) {
  const client = useQueryClient();
  useEffect(
    () =>
      useSessionStore.subscribe((state, previous) => {
        if (
          state.status === "unauthenticated" &&
          previous.status !== "unauthenticated"
        ) {
          void client.cancelQueries();
          client.clear();
        }
      }),
    [client]
  );
  return children;
}
