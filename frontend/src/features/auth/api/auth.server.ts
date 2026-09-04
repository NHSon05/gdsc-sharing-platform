import { cookies } from "next/headers";
import { AUTH_COOKIE_NAMES } from "@/core/session/session.cookies";
import type { UserProfile } from "../types/auth.types";
import { hasRole, isAdmin } from "../utils/rbac";

const BACKEND_INTERNAL_URL =
  process.env.INTERNAL_API_URL ||
  process.env.BACKEND_API_URL ||
  process.env.NEXT_PUBLIC_API_URL ||
  "http://localhost:5184";

/**
 * SERVER-SIDE API: Fetches current user profile directly on Node.js Server.
 * Used exclusively in Server Components, Server Layouts, and Server Actions.
 * Reads accessToken directly from Next.js request cookies().
 */
export async function getCurrentUserServerSide(
  token?: string
): Promise<UserProfile | null> {
  let accessToken = token;

  if (!accessToken) {
    const cookieStore = await cookies();
    accessToken = cookieStore.get(AUTH_COOKIE_NAMES.ACCESS_TOKEN)?.value;
  }

  if (!accessToken) {
    return null;
  }

  try {
    const res = await fetch(`${BACKEND_INTERNAL_URL}/api/auth/me`, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });

    if (!res.ok) {
      return null;
    }

    return (await res.json()) as UserProfile;
  } catch (error) {
    console.error(
      "[getCurrentUserServerSide] Failed to fetch user on server:",
      error
    );
    return null;
  }
}

/**
 * SERVER-SIDE API: Calls backend to rotate refresh token and get a new access token.
 */
export async function refreshTokensServerSide(
  refreshTokenParam?: string
): Promise<{ accessToken: string; refreshToken: string } | null> {
  let refreshToken = refreshTokenParam;

  if (!refreshToken) {
    const cookieStore = await cookies();
    refreshToken = cookieStore.get(AUTH_COOKIE_NAMES.REFRESH_TOKEN)?.value;
  }

  if (!refreshToken) {
    return null;
  }

  try {
    const res = await fetch(`${BACKEND_INTERNAL_URL}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ refreshToken }),
      cache: "no-store",
    });

    if (!res.ok) {
      return null;
    }

    return (await res.json()) as { accessToken: string; refreshToken: string };
  } catch (error) {
    console.error("[refreshTokensServerSide] Failed to refresh tokens on server:", error);
    return null;
  }
}

/**
 * SERVER-SIDE HELPER: Checks if the current server session user has a specific role.
 */
export async function hasRoleServerSide(role: string): Promise<boolean> {
  const user = await getCurrentUserServerSide();
  return hasRole(user, role);
}

/**
 * SERVER-SIDE HELPER: Checks if the current server session user is an Admin.
 */
export async function isAdminServerSide(): Promise<boolean> {
  const user = await getCurrentUserServerSide();
  return isAdmin(user);
}

/**
 * SERVER-SIDE HELPER: Decodes JWT payload without verifying signature (for fast claim checks).
 */
export function decodeJwtPayload<T = Record<string, unknown>>(
  token: string
): T | null {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = Buffer.from(base64, "base64").toString("utf-8");
    return JSON.parse(jsonPayload) as T;
  } catch {
    return null;
  }
}
