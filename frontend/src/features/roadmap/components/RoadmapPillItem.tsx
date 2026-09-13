"use client";

import React from "react";
import Link from "next/link";
import { Bookmark, Archive, FileCheck } from "lucide-react";

interface RoadmapPillItemProps {
  title: string;
  slug: string;
  isBookmarked?: boolean;
  onToggleBookmark?: (slug: string) => void;
  status?: string;
}

export function RoadmapPillItem({
  title,
  slug,
  isBookmarked = false,
  onToggleBookmark,
  status,
}: RoadmapPillItemProps) {
  return (
    <Link
      href={`/roadmaps/${encodeURIComponent(slug)}`}
      className="group relative flex h-13 w-full items-center justify-between rounded-xl border border-neutral-200/90 bg-white/85 px-4 text-neutral-800 shadow-2xs backdrop-blur-md transition-all duration-150 hover:-translate-y-0.5 hover:border-brand/70 hover:shadow-md dark:border-zinc-800/80 dark:bg-[#111827]/70 dark:text-zinc-200 dark:hover:border-zinc-600 dark:hover:bg-zinc-800/70"
    >
      <div className="flex items-center gap-2 min-w-0 pr-2">
        <span className="truncate text-[13.5px] font-semibold tracking-tight transition-colors group-hover:text-brand dark:group-hover:text-white">
          {title}
        </span>

        {status === "Archived" && (
          <span className="flex shrink-0 items-center gap-0.5 rounded-md border border-amber-300 bg-amber-50 px-1.5 py-0.2 text-[9px] font-semibold text-amber-700 dark:border-amber-800 dark:bg-amber-950/40 dark:text-amber-300">
            <Archive className="size-2.5" />
            Archived
          </span>
        )}

        {status === "Draft" && (
          <span className="flex shrink-0 items-center gap-0.5 rounded-md border border-neutral-300 bg-neutral-100 px-1.5 py-0.2 text-[9px] font-semibold text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
            <FileCheck className="size-2.5" />
            Draft
          </span>
        )}
      </div>

      <button
        type="button"
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          onToggleBookmark?.(slug);
        }}
        className={`flex size-7 shrink-0 items-center justify-center rounded-lg transition-colors ${
          isBookmarked
            ? "text-amber-500 fill-amber-500"
            : "text-neutral-400 hover:text-amber-500 dark:text-zinc-500 dark:hover:text-amber-400"
        }`}
        title={isBookmarked ? "Remove from bookmarks" : "Bookmark roadmap"}
      >
        <Bookmark
          className={`size-4 transition-transform group-hover/btn:scale-110 ${
            isBookmarked ? "fill-amber-400 text-amber-400" : ""
          }`}
        />
      </button>
    </Link>
  );
}
