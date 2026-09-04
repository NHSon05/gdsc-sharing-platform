"use client";

import React from "react";
import { format, parseISO } from "date-fns";
import { Calendar, CheckCircle2, History } from "lucide-react";
import type { ClubMembershipDto } from "../types/profile.types";
import { DepartmentMembershipCard } from "./department-membership-card";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface GenerationCardProps {
  membership: ClubMembershipDto;
  className?: string;
}

export function GenerationCard({ membership, className }: GenerationCardProps) {
  const { t } = useTranslation();
  const { generation, isActive, departments = [] } = membership;

  const formatDate = (isoString?: string) => {
    if (!isoString) return null;
    try {
      return format(parseISO(isoString), "MMM yyyy");
    } catch {
      return isoString;
    }
  };

  const startFormatted = formatDate(generation.startDate);
  const endFormatted = formatDate(generation.endDate);
  const dateRange =
    startFormatted && endFormatted
      ? `${startFormatted} - ${endFormatted}`
      : startFormatted || generation.name;

  return (
    <div
      className={cn(
        "rounded-3xl border border-neutral-200/80 bg-neutral-50/50 p-6 shadow-xs transition-all dark:border-zinc-800/80 dark:bg-zinc-900/30",
        isActive && "border-brand/30 bg-brand/[0.02] dark:bg-brand/[0.03]",
        className
      )}
    >
      {/* Generation Card Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="bg-brand text-brand-foreground flex size-10 items-center justify-center rounded-2xl text-sm font-bold shadow-xs">
            G{generation.number}
          </div>

          <div>
            <h3 className="text-base font-bold text-neutral-900 dark:text-white">
              {generation.name || `Gen ${generation.number}`}
            </h3>
            <div className="flex items-center gap-1.5 text-xs text-neutral-500 dark:text-zinc-400">
              <Calendar className="size-3.5" />
              <span>{dateRange}</span>
            </div>
          </div>
        </div>

        {/* Status Badge */}
        <div>
          {isActive ? (
            <span className="inline-flex items-center gap-1.5 rounded-full border border-emerald-500/30 bg-emerald-500/10 px-3 py-1 text-xs font-bold text-emerald-600 dark:bg-emerald-500/15 dark:text-emerald-400">
              <CheckCircle2 className="size-3.5" />
              <span>{t("profile.activeMember")}</span>
            </span>
          ) : (
            <span className="inline-flex items-center gap-1.5 rounded-full border border-neutral-200 bg-neutral-100 px-3 py-1 text-xs font-semibold text-neutral-600 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-400">
              <History className="size-3.5" />
              <span>{t("profile.alumni")}</span>
            </span>
          )}
        </div>
      </div>

      {/* Departments Grid within this Gen */}
      <div className="mt-6">
        {departments.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-neutral-200 p-6 text-center text-xs text-neutral-400 dark:border-zinc-800 dark:text-zinc-500">
            {t("profile.noDepartments")}
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {departments.map((deptMembership) => (
              <DepartmentMembershipCard
                key={deptMembership.id || deptMembership.department.id}
                membership={deptMembership}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
