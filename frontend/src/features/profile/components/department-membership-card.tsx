"use client";

import React from "react";
import { Star, CheckCircle2, XCircle } from "lucide-react";
import type { DepartmentMembershipDto } from "../types/profile.types";
import { RoleBadgeList } from "./role-badge-list";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface DepartmentMembershipCardProps {
  membership: DepartmentMembershipDto;
  className?: string;
}

export function DepartmentMembershipCard({
  membership,
  className,
}: DepartmentMembershipCardProps) {
  const { t } = useTranslation();
  const { department, isPrimary, isActive = true, roles } = membership;

  return (
    <div
      className={cn(
        "group relative flex flex-col justify-between rounded-2xl border border-neutral-200/80 bg-white p-4.5 shadow-xs transition-all hover:shadow-md dark:border-zinc-800/80 dark:bg-zinc-900/80",
        isPrimary && "border-brand/40 ring-brand/10 ring-2",
        className
      )}
    >
      {/* Department Header */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-2.5">
          {/* Color accent dot / badge */}
          <div
            className="size-3.5 shrink-0 rounded-full shadow-xs"
            style={{ backgroundColor: department.color || "#2563EB" }}
          />

          <div>
            <h4 className="text-sm font-bold text-neutral-900 dark:text-white">
              {department.name}
            </h4>
            <p className="font-mono text-[11px] text-neutral-400 dark:text-zinc-500">
              #{department.slug}
            </p>
          </div>
        </div>

        {/* Primary & Active Badges */}
        <div className="flex flex-wrap items-center gap-1.5">
          {isPrimary && (
            <span className="bg-brand/10 text-brand border-brand/20 dark:bg-brand/15 dark:text-brand-hover inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-[10px] font-bold">
              <Star className="size-3 fill-current" />
              <span>{t("profile.primaryDept")}</span>
            </span>
          )}

          {isActive ? (
            <span className="inline-flex items-center gap-1 rounded-full border border-emerald-500/20 bg-emerald-500/10 px-2 py-0.5 text-[10px] font-semibold text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-400">
              <CheckCircle2 className="size-3" />
              <span>{t("profile.activeMember")}</span>
            </span>
          ) : (
            <span className="inline-flex items-center gap-1 rounded-full border border-neutral-200 bg-neutral-100 px-2 py-0.5 text-[10px] font-medium text-neutral-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-400">
              <XCircle className="size-3" />
              <span>{t("profile.alumni")}</span>
            </span>
          )}
        </div>
      </div>

      {/* Description if any */}
      {department.description && (
        <p className="mt-2 line-clamp-2 text-xs leading-relaxed text-neutral-500 dark:text-zinc-400">
          {department.description}
        </p>
      )}

      {/* Roles List */}
      <div className="mt-4 border-t border-neutral-100 pt-3 dark:border-zinc-800/80">
        <span className="mb-1.5 block text-[10px] font-semibold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
          {t("profile.roles")}
        </span>
        <RoleBadgeList roles={roles} />
      </div>
    </div>
  );
}
