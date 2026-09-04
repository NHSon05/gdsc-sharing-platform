"use client";

import React from "react";
import { Sparkles, Layers } from "lucide-react";
import type { ClubMembershipDto } from "../types/profile.types";
import { GenerationCard } from "./generation-card";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface MembershipHistoryProps {
  memberships: ClubMembershipDto[];
  className?: string;
}

export function MembershipHistory({
  memberships = [],
  className,
}: MembershipHistoryProps) {
  const { t } = useTranslation();

  // Sort by generation number descending (Gen 3 -> Gen 2 -> Gen 1)
  const sortedMemberships = React.useMemo(() => {
    return [...memberships].sort(
      (a, b) => (b.generation?.number || 0) - (a.generation?.number || 0)
    );
  }, [memberships]);

  if (sortedMemberships.length === 0) {
    return (
      <div
        className={cn(
          "flex flex-col items-center justify-center rounded-3xl border border-dashed border-neutral-200/90 bg-white/50 p-12 text-center dark:border-zinc-800 dark:bg-zinc-900/30",
          className
        )}
      >
        <div className="bg-brand/10 text-brand dark:bg-brand/15 mb-4 flex size-14 items-center justify-center rounded-2xl">
          <Layers className="size-7" />
        </div>
        <h3 className="text-base font-bold text-neutral-900 dark:text-white">
          {t("profile.noMemberships")}
        </h3>
        <p className="mt-1.5 max-w-sm text-xs text-neutral-500 dark:text-zinc-400">
          Contact your club administrator or chapter lead to be assigned to your
          generation and department.
        </p>
      </div>
    );
  }

  return (
    <div className={cn("space-y-6", className)}>
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-brand size-4.5" />
          <h2 className="text-lg font-bold tracking-tight text-neutral-900 dark:text-white">
            {t("profile.tabClubHistory")}
          </h2>
        </div>
        <span className="rounded-full bg-neutral-100 px-3 py-1 text-xs font-semibold text-neutral-600 dark:bg-zinc-800 dark:text-zinc-300">
          {sortedMemberships.length} {t("profile.generation")}
        </span>
      </div>

      <div className="space-y-6">
        {sortedMemberships.map((membership) => (
          <GenerationCard
            key={membership.id || String(membership.generation?.number)}
            membership={membership}
          />
        ))}
      </div>
    </div>
  );
}
