import type { SessionState } from "./session.types";

export const selectAccessToken = (state: SessionState) => state.accessToken;
export const selectRefreshToken = (state: SessionState) => state.refreshToken;
export const selectCurrentUser = (state: SessionState) => state.user;
export const selectIsAuthenticated = (state: SessionState) =>
  state.status === "authenticated" && Boolean(state.accessToken);
export const selectAuthStatus = (state: SessionState) => state.status;
export const selectRoles = (state: SessionState) => state.user?.roles || [];
