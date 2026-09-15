"use client";

import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export function ProfilePageSkeleton() {
  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-8 sm:px-6 lg:px-8">
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
        {/* Left Column Skeleton */}
        <div className="space-y-6 lg:col-span-8">
          {/* Hero Card Skeleton */}
          <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
            <Skeleton className="h-44 w-full sm:h-52 rounded-none" />
            <div className="px-6 pb-6 pt-2 sm:px-8 space-y-4">
              <div className="-mt-16 sm:-mt-20 flex items-end justify-between">
                <Skeleton className="size-28 sm:size-32 rounded-full border-4 border-white dark:border-[#0C0C0E]" />
                <Skeleton className="h-8 w-28 rounded-full" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-7 w-52 rounded-lg" />
                <Skeleton className="h-4 w-40 rounded-md" />
                <Skeleton className="h-3 w-32 rounded-md" />
              </div>
              <div className="flex gap-3 border-t border-neutral-100 pt-4 dark:border-zinc-800">
                <Skeleton className="size-8 rounded-xl" />
                <Skeleton className="size-8 rounded-xl" />
                <Skeleton className="size-8 rounded-xl" />
                <Skeleton className="size-8 rounded-xl" />
              </div>
            </div>
          </div>

          {/* Tabs Bar Skeleton */}
          <div className="flex gap-2">
            <Skeleton className="h-10 w-44 rounded-xl" />
            <Skeleton className="h-10 w-40 rounded-xl" />
            <Skeleton className="h-10 w-40 rounded-xl" />
          </div>

          {/* Content Skeleton */}
          <div className="space-y-4">
            <Skeleton className="h-48 w-full rounded-2xl" />
            <Skeleton className="h-48 w-full rounded-2xl" />
          </div>
        </div>

        {/* Right Sidebar Skeleton */}
        <div className="space-y-6 lg:col-span-4">
          <Skeleton className="h-56 w-full rounded-3xl" />
          <Skeleton className="h-96 w-full rounded-3xl" />
        </div>
      </div>
    </div>
  );
}
