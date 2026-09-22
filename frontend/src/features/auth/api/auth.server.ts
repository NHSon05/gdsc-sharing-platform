import "server-only";
import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import { backendFetch } from "@/core/session/bff.server";
import { parseUserProfile } from "@/core/session/user-profile";
import type { UserProfile } from "../types/auth.types";
import { hasRole, isAdmin } from "../utils/rbac";

// Read-only: RSC must never rotate tokens because it cannot persist response cookies.
export async function getCurrentUserServerSide(): Promise<UserProfile | null> {
  const token = (await cookies()).get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  if (!token) return null;
  try {
    const response = await backendFetch("/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` },
    });
    return response.ok ? parseUserProfile(await response.json()) : null;
  } catch {
    return null;
  }
}
export async function hasRoleServerSide(role: string) {
  return hasRole(await getCurrentUserServerSide(), role);
}
export async function isAdminServerSide() {
  return isAdmin(await getCurrentUserServerSide());
}
