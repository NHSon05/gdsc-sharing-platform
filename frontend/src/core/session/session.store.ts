import { create } from "zustand";
import type { SessionState } from "./session.types";

// UX state only. Credentials are exclusively owned by the BFF's HttpOnly cookies.
export const useSessionStore = create<SessionState>((set) => ({
  revision: 0,
  user: null,
  status: "idle",
  setUser: (user) =>
    set({ user, status: user ? "authenticated" : "unauthenticated" }),
  clearSession: () =>
    set((state) => ({
      user: null,
      status: "unauthenticated",
      revision: state.revision + 1,
    })),
}));
