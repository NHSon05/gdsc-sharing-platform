"use client";

import React from "react";
import { useTranslation } from "@/core/i18n/i18n.context";
import type { TagResponse } from "../../types/sharing.types";
import { Search, X } from "lucide-react";

interface ContentFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  selectedTagId?: string;
  onTagChange: (tagId: string | undefined) => void;
  tags?: TagResponse[];
  sort: string;
  onSortChange: (sort: "newest" | "oldest" | "title") => void;
  className?: string;
}

export function ContentFilterBar({
  search,
  onSearchChange,
  selectedTagId,
  onTagChange,
  tags = [],
  sort,
  onSortChange,
  className,
}: ContentFilterBarProps) {
  const { t } = useTranslation();

  const hasFilters = Boolean(search.trim() || selectedTagId);

  return (
    <div
      className={`flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between ${
        className ?? ""
      }`}
    >
      {/* Left: Search input */}
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder={t("sharing.searchPlaceholder")}
          className="focus:border-brand focus:ring-brand dark:focus:border-brand h-10 w-full rounded-xl border border-neutral-200 bg-white pr-9 pl-10 text-sm text-neutral-900 shadow-2xs placeholder:text-neutral-400 focus:ring-1 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
        />
        {search && (
          <button
            type="button"
            onClick={() => onSearchChange("")}
            className="hover:text-brand dark:hover:text-brand absolute top-1/2 right-3 -translate-y-1/2 text-neutral-400 dark:text-zinc-500"
          >
            <X className="size-4" />
          </button>
        )}
      </div>

      {/* Right: Tag and Sort dropdowns */}
      <div className="flex flex-wrap items-center gap-2">
        {/* Tag selector */}
        <select
          value={selectedTagId ?? ""}
          onChange={(e) => onTagChange(e.target.value || undefined)}
          className="focus:border-brand dark:focus:border-brand h-10 rounded-xl border border-neutral-200 bg-white px-3 text-xs font-medium text-neutral-700 shadow-2xs focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
        >
          <option value="">{t("sharing.selectTags")} (All)</option>
          {tags.map((tag) => (
            <option key={tag.id} value={tag.id}>
              {tag.name}
            </option>
          ))}
        </select>

        {/* Sort selector */}
        <select
          value={sort}
          onChange={(e) =>
            onSortChange(e.target.value as "newest" | "oldest" | "title")
          }
          className="focus:border-brand dark:focus:border-brand h-10 rounded-xl border border-neutral-200 bg-white px-3 text-xs font-medium text-neutral-700 shadow-2xs focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
        >
          <option value="newest">Newest First</option>
          <option value="oldest">Oldest First</option>
          <option value="title">Title (A-Z)</option>
        </select>

        {/* Reset button if filters active */}
        {hasFilters && (
          <button
            type="button"
            onClick={() => {
              onSearchChange("");
              onTagChange(undefined);
            }}
            className="border-brand-border/40 bg-brand-muted text-brand hover:bg-brand flex h-10 items-center gap-1 rounded-xl border px-3 text-xs font-semibold transition-colors hover:text-white dark:border-zinc-800"
          >
            <X className="size-3.5" />
            <span>Reset</span>
          </button>
        )}
      </div>
    </div>
  );
}
