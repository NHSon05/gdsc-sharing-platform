"use client";

import React from "react";
import type { ClubRoleDto } from "../types/profile.types";
import { cn } from "@/lib/utils";

export interface RoleBadgeListProps {
  roles: ClubRoleDto[];
  className?: string;
}

/**
 * Returns tailored color styles based on role code or name
 */
export function getRoleBadgeStyle(codeOrName: string) {
  const norm = codeOrName.toUpperCase().replace(/\s+/g, "");

  if (norm.includes("LEAD") && !norm.includes("SUB")) {
    // Lead - Amber / Gold
    return "bg-amber-500/10 text-amber-700 border-amber-500/30 dark:bg-amber-500/15 dark:text-amber-400 dark:border-amber-500/30 font-bold";
  }
  if (norm.includes("SUBLEAD") || norm.includes("SUB_LEAD")) {
    // Sub Lead - Emerald / Teal
    return "bg-teal-500/10 text-teal-700 border-teal-500/30 dark:bg-teal-500/15 dark:text-teal-400 dark:border-teal-500/30 font-semibold";
  }
  if (norm.includes("CORE") || norm.includes("MEMBER")) {
    // Core Team / Member - Brand Indigo / Sky
    return "bg-brand/10 text-brand border-brand/30 dark:bg-brand/15 dark:text-brand-hover dark:border-brand/30 font-semibold";
  }

  // Default neutral role
  return "bg-neutral-100 text-neutral-700 border-neutral-200 dark:bg-zinc-800 dark:text-zinc-300 dark:border-zinc-700 font-medium";
}

export function RoleBadgeList({ roles = [], className }: RoleBadgeListProps) {
  if (!roles || roles.length === 0) {
    return (
      <span className="text-xs text-neutral-400 italic dark:text-zinc-500">
        No specific role
      </span>
    );
  }

  return (
    <div className={cn("flex flex-wrap items-center gap-1.5", className)}>
      {roles.map((role) => (
        <span
          key={role.id || role.code}
          className={cn(
            "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs tracking-tight shadow-2xs transition-all select-none",
            getRoleBadgeStyle(role.code || role.name)
          )}
        >
          {role.name || role.code}
        </span>
      ))}
    </div>
  );
}
