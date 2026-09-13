"use client";

import React from "react";
import { Search, X } from "lucide-react";

interface RoadmapFiltersProps {
  search: string;
  onSearchChange: (val: string) => void;
}

export function RoadmapFilters({
  search,
  onSearchChange,
}: RoadmapFiltersProps) {
  return (
    <div className="relative w-full max-w-md">
      <Search className="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
      <input
        type="text"
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        placeholder="Search roadmaps by title..."
        className="h-10 w-full rounded-full border border-neutral-200/90 bg-white/80 pl-10 pr-10 text-sm text-neutral-900 shadow-2xs backdrop-blur-md transition-all placeholder:text-neutral-400 focus:border-brand focus:outline-none focus:ring-2 focus:ring-brand/20 dark:border-zinc-800 dark:bg-zinc-900/80 dark:text-zinc-100 dark:placeholder:text-zinc-500"
      />
      {search && (
        <button
          type="button"
          onClick={() => onSearchChange("")}
          className="absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 dark:text-zinc-500 dark:hover:text-zinc-300"
          aria-label="Clear search"
        >
          <X className="size-4" />
        </button>
      )}
    </div>
  );
}
