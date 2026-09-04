"use client";

import React from "react";
import { useGenerationsQuery } from "../hooks/use-lookup-queries";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

export interface GenerationSelectorProps {
  value: string;
  onChange: (generationId: string) => void;
  includeInactive?: boolean;
  disabled?: boolean;
  className?: string;
}

export function GenerationSelector({
  value,
  onChange,
  includeInactive = false,
  disabled = false,
  className,
}: GenerationSelectorProps) {
  const { t } = useTranslation();
  const { data: generations = [], isLoading } =
    useGenerationsQuery(includeInactive);

  return (
    <div className={cn("relative w-full", className)}>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled || isLoading}
        className="focus:border-brand focus:ring-brand/20 w-full appearance-none rounded-2xl border border-neutral-200/90 bg-white px-3.5 py-2.5 text-xs font-medium text-neutral-900 transition-all focus:ring-2 focus:outline-hidden disabled:opacity-50 dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100"
      >
        <option value="">
          {isLoading ? "Đang tải nhiệm kỳ..." : t("memberManagement.selectGen")}
        </option>
        {generations.map((gen) => (
          <option key={gen.id} value={gen.id}>
            {gen.name || `Gen ${gen.number}`}{" "}
            {!gen.isActive ? " (Inactive)" : ""}
          </option>
        ))}
      </select>
    </div>
  );
}
