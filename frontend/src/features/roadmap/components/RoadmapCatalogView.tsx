"use client";

import React, { useState, useMemo } from "react";
import { useRoadmapsQuery } from "../hooks/use-roadmaps-query";
import { useRoadmapCategoriesQuery } from "../hooks/use-roadmap-categories-query";
import { RoadmapPillItem } from "./RoadmapPillItem";
import { RoadmapCard } from "./RoadmapCard";
import { RoadmapFilters } from "./RoadmapFilters";
import { CreateRoadmapDialog } from "./modals/CreateRoadmapDialog";
import { Button } from "@/components/ui/button";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import { isAdmin } from "@/features/auth/utils/rbac";
import {
  PlusCircle,
  LayoutGrid,
  List,
  Bookmark,
  FilterX,
  RefreshCw,
  Compass,
} from "lucide-react";

const BOOKMARKS_STORAGE_KEY = "gdsc_roadmap_bookmarks";

export function RoadmapCatalogView() {
  const user = useSessionStore(selectCurrentUser);
  const userIsAdmin = isAdmin(user);

  const [search, setSearch] = useState("");
  const [viewMode, setViewMode] = useState<"pill" | "card">("pill");
  const [showBookmarksOnly, setShowBookmarksOnly] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  // Lazy initialize bookmarks from localStorage
  const [bookmarkedSlugs, setBookmarkedSlugs] = useState<Set<string>>(() => {
    if (typeof window === "undefined") return new Set();
    try {
      const stored = localStorage.getItem(BOOKMARKS_STORAGE_KEY);
      return stored ? new Set(JSON.parse(stored)) : new Set();
    } catch {
      return new Set();
    }
  });

  const toggleBookmark = (slug: string) => {
    setBookmarkedSlugs((prev) => {
      const next = new Set(prev);
      if (next.has(slug)) {
        next.delete(slug);
      } else {
        next.add(slug);
      }
      try {
        localStorage.setItem(
          BOOKMARKS_STORAGE_KEY,
          JSON.stringify(Array.from(next))
        );
      } catch {
        // ignore
      }
      return next;
    });
  };

  const { data: categories = [] } = useRoadmapCategoriesQuery();
  const {
    data: roadmapsPage,
    isLoading,
    isError,
    refetch,
  } = useRoadmapsQuery({
    search: search.trim() || undefined,
    page: 1,
    pageSize: 100,
  });

  // Strictly use real database data from backend
  const allRoadmaps = useMemo(
    () => roadmapsPage?.items ?? [],
    [roadmapsPage?.items]
  );

  const filteredRoadmaps = useMemo(() => {
    if (!showBookmarksOnly) return allRoadmaps;
    return allRoadmaps.filter((rm) => bookmarkedSlugs.has(rm.slug));
  }, [allRoadmaps, showBookmarksOnly, bookmarkedSlugs]);

  const handleResetFilters = () => {
    setSearch("");
    setShowBookmarksOnly(false);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Top Header Controls */}
      <div className="space-y-6">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-2xl font-extrabold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
              Role-based Roadmaps
            </h1>
            <p className="mt-1 text-xs text-neutral-500 sm:text-sm dark:text-zinc-400">
              Explore structured learning paths and technology roadmaps.
            </p>
          </div>

          {/* Action Bar */}
          <div className="flex flex-wrap items-center gap-2.5">
            {/* View Mode Switcher */}
            <div className="flex h-10 items-center rounded-full border border-neutral-200/90 bg-white p-1 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900">
              <button
                type="button"
                onClick={() => setViewMode("pill")}
                className={`flex h-8 w-8 cursor-pointer items-center justify-center rounded-full transition-all duration-200 ${
                  viewMode === "pill"
                    ? "bg-brand text-brand-foreground shadow-[0_2px_8px_var(--brand-glow)]"
                    : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
                }`}
                title="Pill Grid View"
                aria-label="Pill Grid View"
              >
                <LayoutGrid className="size-4" />
              </button>

              <button
                type="button"
                onClick={() => setViewMode("card")}
                className={`flex h-8 w-8 cursor-pointer items-center justify-center rounded-full transition-all duration-200 ${
                  viewMode === "card"
                    ? "bg-brand text-brand-foreground shadow-[0_2px_8px_var(--brand-glow)]"
                    : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
                }`}
                title="Detailed Card View"
                aria-label="Detailed Card View"
              >
                <List className="size-4" />
              </button>
            </div>

            {/* Bookmarks Filter Pill */}
            <Button
              variant="outline"
              size="md"
              onClick={() => setShowBookmarksOnly(!showBookmarksOnly)}
              leftIcon={
                <Bookmark
                  className={`size-4 ${
                    showBookmarksOnly
                      ? "fill-amber-400 text-amber-500"
                      : "text-neutral-700 dark:text-zinc-300"
                  }`}
                />
              }
              className={`font-medium shadow-2xs ${
                showBookmarksOnly
                  ? "border-amber-400 bg-amber-50 text-amber-800 hover:bg-amber-100/70 dark:border-amber-600 dark:bg-amber-950/40 dark:text-amber-300"
                  : "border-neutral-200/90 bg-white text-neutral-700 hover:bg-neutral-50 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-200"
              }`}
            >
              Favorites
              {bookmarkedSlugs.size > 0 && (
                <span className="ml-1 rounded-full bg-amber-100 px-1.5 py-0.5 text-[10px] font-bold text-amber-800 dark:bg-amber-950 dark:text-amber-300">
                  {bookmarkedSlugs.size}
                </span>
              )}
            </Button>

            {/* Admin Create Action */}
            {userIsAdmin && (
              <Button
                variant="brand"
                size="md"
                leftIcon={<PlusCircle className="size-4" />}
                onClick={() => setIsCreateOpen(true)}
                className="font-semibold shadow-md"
              >
                Create Roadmap
              </Button>
            )}
          </div>
        </div>

        {/* Search Bar */}
        <RoadmapFilters search={search} onSearchChange={setSearch} />
      </div>

      {/* Roadmaps Grid */}
      <div className="mt-8">
        {isLoading ? (
          <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2 lg:grid-cols-3">
            {[1, 2, 3, 4, 5, 6].map((idx) => (
              <div
                key={idx}
                className="h-13 animate-pulse rounded-xl border border-neutral-200/60 bg-neutral-100/70 dark:border-zinc-800 dark:bg-zinc-900/60"
              />
            ))}
          </div>
        ) : isError ? (
          <div className="flex flex-col items-center justify-center rounded-2xl border border-rose-200/80 bg-rose-50/50 p-12 text-center dark:border-rose-900/40 dark:bg-rose-950/20">
            <p className="text-sm font-semibold text-rose-700 dark:text-rose-400">
              Failed to load roadmaps from the server.
            </p>
            <Button
              variant="outline"
              size="sm"
              onClick={() => refetch()}
              className="mt-4 gap-1.5 rounded-full"
            >
              <RefreshCw className="size-3.5" />
              <span>Retry</span>
            </Button>
          </div>
        ) : filteredRoadmaps.length === 0 ? (
          <div className="flex flex-col items-center justify-center rounded-3xl border border-dashed border-neutral-300/80 bg-neutral-50/40 p-16 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
            <div className="flex size-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500">
              {search || showBookmarksOnly ? (
                <FilterX className="size-7" />
              ) : (
                <Compass className="size-7" />
              )}
            </div>
            <h3 className="mt-4 text-lg font-bold text-neutral-900 dark:text-zinc-100">
              {search || showBookmarksOnly
                ? "No matching roadmaps found"
                : "No roadmaps in the database"}
            </h3>
            <p className="mt-1.5 max-w-sm text-sm text-neutral-500 dark:text-zinc-400">
              {search || showBookmarksOnly
                ? "Try adjusting your search query or removing the favorites filter."
                : userIsAdmin
                  ? "Create your first roadmap using the button below to get started."
                  : "No roadmaps have been published yet. Please check back later."}
            </p>
            {search || showBookmarksOnly ? (
              <Button
                variant="outline"
                size="sm"
                onClick={handleResetFilters}
                className="mt-5 rounded-full"
              >
                Clear filters
              </Button>
            ) : userIsAdmin ? (
              <Button
                variant="brand"
                size="md"
                leftIcon={<PlusCircle className="size-4" />}
                onClick={() => setIsCreateOpen(true)}
                className="mt-5 font-semibold shadow-md"
              >
                Create Roadmap
              </Button>
            ) : null}
          </div>
        ) : viewMode === "pill" ? (
          /* 3-Column Pill Grid */
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {filteredRoadmaps.map((rm) => (
              <RoadmapPillItem
                key={rm.id || rm.slug}
                title={rm.title}
                slug={rm.slug}
                isBookmarked={bookmarkedSlugs.has(rm.slug)}
                onToggleBookmark={toggleBookmark}
                status={rm.status}
              />
            ))}
          </div>
        ) : (
          /* Detailed Card View */
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {filteredRoadmaps.map((rm) => (
              <RoadmapCard key={rm.id || rm.slug} roadmap={rm} />
            ))}
          </div>
        )}
      </div>

      {/* Admin Create Roadmap Dialog */}
      {userIsAdmin && (
        <CreateRoadmapDialog
          open={isCreateOpen}
          onOpenChange={setIsCreateOpen}
          categories={categories}
        />
      )}
    </div>
  );
}
