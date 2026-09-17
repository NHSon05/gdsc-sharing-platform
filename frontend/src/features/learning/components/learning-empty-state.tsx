"use client";

import React from "react";
import { Search, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface LearningEmptyStateProps {
  searchTerm?: string;
  onReset: () => void;
  className?: string;
}

export function LearningEmptyState({
  searchTerm,
  onReset,
  className = "",
}: LearningEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <div
      className={`flex min-h-72 flex-col items-center justify-center rounded-3xl border border-dashed border-neutral-200 bg-white p-8 text-center dark:border-zinc-800 dark:bg-[#0C0C0E] ${className}`}
    >
      <div className="flex size-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500">
        <Search className="size-7" />
      </div>

      <h3 className="mt-3 text-sm font-bold text-neutral-800 dark:text-zinc-200">
        {searchTerm
          ? t("learning.notFoundTitle").replace("{search}", searchTerm)
          : t("learning.emptyCategoryTitle")}
      </h3>

      <p className="mt-1 max-w-sm text-xs text-neutral-500 dark:text-zinc-400">
        {t("learning.emptyDescription")}
      </p>

      <Button
        variant="outline"
        size="sm"
        onClick={onReset}
        className="mt-4 text-xs font-semibold"
      >
        <RotateCcw className="mr-1.5 size-3.5" />
        <span>{t("learning.viewAllQuestions")}</span>
      </Button>
    </div>
  );
}
