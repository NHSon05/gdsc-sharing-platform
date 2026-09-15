"use client";

import React from "react";
import { Mail, Shield, Sparkles, Layers } from "lucide-react";
import type { UserProfileDto } from "../../types/profile.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface ProfileUserInfoProps {
  profile: UserProfileDto;
  onOpenEmailChange: () => void;
  className?: string;
}

export function ProfileUserInfo({
  profile,
  onOpenEmailChange,
  className = "",
}: ProfileUserInfoProps) {
  const { t } = useTranslation();

  const activeMembership = profile.memberships?.find((m) => m.isActive);
  const primaryDept =
    activeMembership?.departments?.find((d) => d.isPrimary) ||
    activeMembership?.departments?.[0];

  return (
    <div className={`space-y-2 ${className}`}>
      {/* Name and System Role */}
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl dark:text-white">
          {profile.displayName}
        </h1>

        <span className="bg-brand/10 text-brand border-brand/30 dark:bg-brand/15 dark:text-brand-hover inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-semibold">
          <Shield className="size-3.5" />
          <span>{profile.systemRoles || "Member"}</span>
        </span>
      </div>

      {/* Email and Trigger */}
      <div className="flex flex-wrap items-center gap-2 text-xs text-neutral-500 dark:text-zinc-400">
        <div className="flex items-center gap-1.5">
          <Mail className="size-3.5" />
          <span>{profile.email}</span>
        </div>
        <span>•</span>
        <button
          type="button"
          onClick={onOpenEmailChange}
          className="text-brand hover:text-brand-hover cursor-pointer font-semibold underline-offset-2 hover:underline"
        >
          {t("profile.emailChangeTitle")}
        </button>
      </div>

      {/* Active Gen & Department Chips */}
      <div className="flex flex-wrap items-center gap-2 pt-1">
        {activeMembership && (
          <span className="inline-flex items-center gap-1.5 rounded-full border border-neutral-200 bg-neutral-100 px-3 py-0.5 text-xs font-medium text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
            <Layers className="size-3 text-neutral-400" />
            <span>
              {activeMembership.generation.name ||
                `Gen ${activeMembership.generation.number}`}
            </span>
          </span>
        )}

        {primaryDept && (
          <span className="bg-brand-muted text-brand inline-flex items-center gap-1.5 rounded-full px-3 py-0.5 text-xs font-semibold">
            <Sparkles className="size-3" />
            <span>{primaryDept.department.name}</span>
          </span>
        )}
      </div>
    </div>
  );
}
