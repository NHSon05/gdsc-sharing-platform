"use client";

import React from "react";
import { useCurrentUserQuery } from "@/features/auth/hooks/use-current-user-query";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import { hasAnyRole } from "@/features/auth/utils/rbac";

export interface RoleGuardProps {
  /** List of allowed roles that can view the children (e.g. ['Admin']) */
  allowedRoles: string[];
  /** Fallback UI when user does not have permission */
  fallback?: React.ReactNode;
  /** Protected content */
  children: React.ReactNode;
}

/**
 * Client-Side Role Guard Component:
 * Conditionally renders children only if the logged-in user possesses at least one of the allowed roles.
 */
export function RoleGuard({
  allowedRoles,
  fallback = null,
  children,
}: RoleGuardProps) {
  const { data: queriedUser } = useCurrentUserQuery();
  const storeUser = useSessionStore(selectCurrentUser);
  const user = queriedUser || storeUser;

  const isAllowed = hasAnyRole(user, allowedRoles);

  if (!isAllowed) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
