"use client";

import React from "react";
import { Layers, CheckCircle2, Clock, FileEdit, Archive } from "lucide-react";
import type { SharingContentStatus } from "@/features/sharing";

export type BlogFilterStatus = "ALL" | SharingContentStatus;

export interface ProfileBlogFiltersProps {
  activeFilter: BlogFilterStatus;
  onFilterChange: (status: BlogFilterStatus) => void;
  searchTerm: string;
  onSearchChange: (search: string) => void;
  className?: string;
}

const FILTER_TABS: {
  id: BlogFilterStatus;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
}[] = [
  { id: "ALL", label: "Tất cả bài viết", icon: Layers },
  { id: "Published", label: "Đã xuất bản", icon: CheckCircle2 },
  { id: "PendingReview", label: "Chờ kiểm duyệt", icon: Clock },
  { id: "Draft", label: "Bản nháp", icon: FileEdit },
  { id: "Archived", label: "Đã lưu trữ", icon: Archive },
];

export function ProfileBlogFilters({
  activeFilter,
  onFilterChange,
  className = "",
}: ProfileBlogFiltersProps) {
  return (
    <div
      className={`flex flex-col gap-3 rounded-2xl border border-neutral-200/80 bg-white p-3.5 shadow-2xs sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800/80 dark:bg-zinc-900/60 ${className}`}
    >
      {/* Status Filter Pills */}
      <div className="flex flex-wrap items-center gap-1.5">
        {FILTER_TABS.map((tab) => {
          const Icon = tab.icon;
          const isActive = activeFilter === tab.id;
          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => onFilterChange(tab.id)}
              className={`flex cursor-pointer items-center gap-1.5 rounded-xl px-3 py-1.5 text-xs font-semibold transition-all ${
                isActive
                  ? "bg-brand text-white shadow-2xs"
                  : "text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-zinc-100"
              }`}
            >
              <Icon className="size-3.5" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* Search Input */}
      {/* <div className="relative w-full sm:max-w-xs">
        <Search className="absolute top-1/2 left-3 size-3.5 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
        <input
          type="text"
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="Tìm theo tiêu đề bài viết..."
          className="focus:border-brand w-full rounded-xl border border-neutral-200/90 bg-neutral-50/50 py-1.5 pr-3 pl-8 text-xs text-neutral-900 placeholder:text-neutral-400 focus:bg-white focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100"
        />
      </div> */}
    </div>
  );
}
