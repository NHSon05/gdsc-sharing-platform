import type { SessionState } from "./session.types";
export const selectCurrentUser = (state: SessionState) => state.user;
export const selectIsAuthenticated = (state: SessionState) =>
  state.status === "authenticated";
export const selectAuthStatus = (state: SessionState) => state.status;
export const selectRoles = (state: SessionState) => state.user?.roles || [];
