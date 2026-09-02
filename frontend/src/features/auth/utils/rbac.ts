import type { CurrentUserDto } from "../types/auth.types";

export const ROLES = {
  ADMIN: "Admin",
  MEMBER: "Member",
  LEAD: "Lead",
} as const;

export type UserRole = (typeof ROLES)[keyof typeof ROLES] | string;

/**
 * Checks if a user has a specific role (case-insensitive check against user.roles array).
 */
export function hasRole(
  user: CurrentUserDto | null | undefined,
  role: string
): boolean {
  if (!user || !Array.isArray(user.roles)) return false;
  const target = role.toLowerCase();
  return user.roles.some(
    (r) => typeof r === "string" && r.toLowerCase() === target
  );
}

/**
 * Checks if a user has any of the given roles.
 */
export function hasAnyRole(
  user: CurrentUserDto | null | undefined,
  roles: string[]
): boolean {
  if (!user || !Array.isArray(user.roles)) return false;
  return roles.some((role) => hasRole(user, role));
}

/**
 * Checks if a user has Admin role (even if user also has Member role).
 */
export function isAdmin(user: CurrentUserDto | null | undefined): boolean {
  return hasRole(user, ROLES.ADMIN);
}

/**
 * Checks if a user has Member role.
 */
export function isMember(user: CurrentUserDto | null | undefined): boolean {
  return hasRole(user, ROLES.MEMBER);
}
