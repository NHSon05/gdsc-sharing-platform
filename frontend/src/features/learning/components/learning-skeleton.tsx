"use client";

import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export function LearningSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 8 }).map((_, i) => (
        <div
          key={i}
          className="flex items-center justify-between rounded-2xl border border-neutral-200/80 bg-white p-4.5 dark:border-zinc-800 dark:bg-[#0C0C0E]"
        >
          <div className="flex items-center gap-3">
            <Skeleton className="h-4 w-6 rounded-md" />
            <Skeleton className="h-4 w-64 sm:w-96 rounded-md" />
          </div>
          <div className="flex items-center gap-3">
            <Skeleton className="h-5 w-16 rounded-md" />
            <Skeleton className="size-7 rounded-lg" />
            <Skeleton className="size-7 rounded-lg" />
            <Skeleton className="size-7 rounded-lg" />
          </div>
        </div>
      ))}
    </div>
  );
}
