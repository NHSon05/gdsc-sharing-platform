"use client";

import React from "react";
import { CheckCircle2, AlertCircle } from "lucide-react";
import { Progress } from "@/components/ui/progress";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface ProfileCompletionCardProps {
  completionPercentage?: number;
  missingFields?: string[];
  className?: string;
}

export function ProfileCompletionCard({
  completionPercentage = 100,
  missingFields = [],
  className = "",
}: ProfileCompletionCardProps) {
  const { t } = useTranslation();

  return (
    <div
      className={`flex w-full flex-col gap-2 rounded-2xl border border-neutral-200/80 bg-neutral-50/70 p-4 sm:max-w-xs dark:border-zinc-800 dark:bg-zinc-900/60 ${className}`}
    >
      <div className="flex items-center justify-between text-xs">
        <span className="flex items-center gap-1.5 font-semibold text-neutral-700 dark:text-zinc-300">
          {completionPercentage === 100 ? (
            <CheckCircle2 className="size-3.5 text-emerald-500" />
          ) : (
            <AlertCircle className="size-3.5 text-amber-500" />
          )}
          {t("profile.completionTitle")}
        </span>
        <span className="text-brand font-bold">{completionPercentage}%</span>
      </div>

      <Progress value={completionPercentage} className="h-2" />

      {missingFields.length > 0 && (
        <div className="text-[11px] leading-tight text-neutral-400 dark:text-zinc-500">
          <span className="font-medium text-neutral-500 dark:text-zinc-400">
            {t("profile.missingFields")}{" "}
          </span>
          <span className="italic">{missingFields.join(", ")}</span>
        </div>
      )}
    </div>
  );
}
