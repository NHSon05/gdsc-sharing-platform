"use client";

import React from "react";
import { ContentCard } from "./ContentCard";
import type { ContentSummary } from "../../types/sharing.types";
import { useTranslation } from "@/core/i18n/i18n.context";
import { BookOpen, ChevronLeft, ChevronRight } from "lucide-react";

interface ContentListProps {
  items?: ContentSummary[];
  isLoading?: boolean;
  totalCount?: number;
  page?: number;
  pageSize?: number;
  onPageChange?: (newPage: number) => void;
  showStatus?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  className?: string;
}

function ContentCardSkeleton() {
  return (
    <div className="flex animate-pulse flex-col overflow-hidden rounded-2xl border border-neutral-200 bg-white p-0 dark:border-zinc-800 dark:bg-zinc-900/60">
      <div className="aspect-16/9 w-full bg-neutral-200 dark:bg-zinc-800" />
      <div className="flex flex-1 flex-col space-y-3 p-5">
        <div className="flex gap-2">
          <div className="h-5 w-16 rounded bg-neutral-200 dark:bg-zinc-800" />
          <div className="h-5 w-20 rounded bg-neutral-200 dark:bg-zinc-800" />
        </div>
        <div className="h-6 w-3/4 rounded bg-neutral-200 dark:bg-zinc-800" />
        <div className="h-4 w-full rounded bg-neutral-100 dark:bg-zinc-800/60" />
        <div className="h-4 w-2/3 rounded bg-neutral-100 dark:bg-zinc-800/60" />
        <div className="mt-auto flex justify-between border-t border-neutral-100 pt-4 dark:border-zinc-800">
          <div className="h-4 w-24 rounded bg-neutral-200 dark:bg-zinc-800" />
          <div className="h-4 w-16 rounded bg-neutral-200 dark:bg-zinc-800" />
        </div>
      </div>
    </div>
  );
}

export function ContentList({
  items = [],
  isLoading = false,
  totalCount = 0,
  page = 1,
  pageSize = 20,
  onPageChange,
  showStatus = false,
  emptyTitle,
  emptyDescription,
  className,
}: ContentListProps) {
  const { t } = useTranslation();

  if (isLoading) {
    return (
      <div
        className={`grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 ${className ?? ""}`}
      >
        {Array.from({ length: 6 }).map((_, i) => (
          <ContentCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="flex min-h-80 flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
        <div className="flex size-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500">
          <BookOpen className="size-7" />
        </div>
        <h3 className="mt-4 text-base font-bold text-neutral-900 dark:text-white">
          {emptyTitle || t("sharing.emptyContents")}
        </h3>
        <p className="mt-1 max-w-sm text-xs text-neutral-500 dark:text-zinc-400">
          {emptyDescription || t("sharing.emptyContentsDesc")}
        </p>
      </div>
    );
  }

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-8">
      <div
        className={`grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 ${className ?? ""}`}
      >
        {items.map((item) => (
          <ContentCard
            key={item.id}
            content={item}
            showStatus={showStatus}
            href={
              showStatus
                ? `/sharing/mine?edit=${item.id}`
                : `/sharing/${item.slug}`
            }
          />
        ))}
      </div>

      {/* Pagination Controls */}
      {totalPages > 1 && onPageChange && (
        <div className="flex items-center justify-center gap-2 pt-4">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
            className="flex size-9 cursor-pointer items-center justify-center rounded-xl border border-neutral-200 bg-white text-neutral-700 shadow-2xs hover:bg-neutral-50 disabled:cursor-not-allowed disabled:opacity-40 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300 dark:hover:bg-zinc-800"
          >
            <ChevronLeft className="size-4" />
          </button>
          <span className="px-3 text-xs font-semibold text-neutral-600 dark:text-zinc-400">
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
            className="flex size-9 cursor-pointer items-center justify-center rounded-xl border border-neutral-200 bg-white text-neutral-700 shadow-2xs hover:bg-neutral-50 disabled:cursor-not-allowed disabled:opacity-40 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300 dark:hover:bg-zinc-800"
          >
            <ChevronRight className="size-4" />
          </button>
        </div>
      )}
    </div>
  );
}
