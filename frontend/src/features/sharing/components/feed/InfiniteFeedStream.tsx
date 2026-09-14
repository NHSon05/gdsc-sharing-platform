"use client";

import React, { useState, useEffect, useRef } from "react";
import { useInfiniteContentsQuery } from "../../hooks/use-contents-query";
import { useTagsQuery } from "../../hooks/use-tags-query";
import { FeedPostCard } from "./FeedPostCard";
import { TextField } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  Search,
  Sparkles,
  Loader2,
  CheckCircle,
  BookOpen,
  Filter,
  RefreshCw,
} from "lucide-react";

interface InfiniteFeedStreamProps {
  className?: string;
}

export function InfiniteFeedStream({ className }: InfiniteFeedStreamProps) {
  const { t } = useTranslation();

  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [selectedTagId, setSelectedTagId] = useState<string | undefined>(
    undefined
  );

  // Debounce search input
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(searchInput.trim());
    }, 400);
    return () => clearTimeout(handler);
  }, [searchInput]);

  // Tags for pill filters
  const { data: tags = [] } = useTagsQuery();

  // Infinite contents query (5 items per batch/page)
  const {
    data,
    isLoading,
    isError,
    error,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
    refetch,
  } = useInfiniteContentsQuery(
    {
      search: debouncedSearch || undefined,
      tagId: selectedTagId,
      sort: "newest",
    },
    { pageSize: 5 }
  );

  const allPosts = data?.pages.flatMap((page) => page.items) ?? [];
  const totalCount = data?.pages[0]?.totalCount ?? 0;

  // Infinite scroll trigger via IntersectionObserver
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel || !hasNextPage || isFetchingNextPage) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          fetchNextPage();
        }
      },
      { threshold: 0.1, rootMargin: "250px" }
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  return (
    <div className={`space-y-6 ${className ?? ""}`}>
      {/* Feed Controls: Search & Tag Pills */}
      <div className="space-y-3">
        {/* Search Bar */}
        <div className="relative">
          <TextField
            placeholder={t("sharing.searchPlaceholder")}
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            startIcon={<Search className="size-4 text-neutral-400" />}
            clearable
            onClear={() => setSearchInput("")}
            className="focus:border-brand w-full rounded-xl border-neutral-200 bg-white shadow-2xs dark:border-zinc-800 dark:bg-zinc-900"
          />
        </div>

        {/* Tag Pills Filter */}
        {tags.length > 0 && (
          <div className="flex scrollbar-none items-center gap-1.5 overflow-x-auto pb-1.5">
            <button
              type="button"
              onClick={() => setSelectedTagId(undefined)}
              className={`shrink-0 rounded-full px-3.5 py-1 text-xs font-semibold transition-all ${
                selectedTagId === undefined
                  ? "bg-brand text-white shadow-xs"
                  : "hover:border-brand/40 hover:text-brand dark:hover:border-brand/40 dark:hover:text-brand border border-neutral-200/80 bg-white text-neutral-600 hover:bg-neutral-50 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
              }`}
            >
              {t("sharing.allTags")}
            </button>

            {tags.map((tag) => (
              <button
                key={tag.id}
                type="button"
                onClick={() =>
                  setSelectedTagId(
                    selectedTagId === tag.id ? undefined : tag.id
                  )
                }
                className={`shrink-0 rounded-full px-3.5 py-1 text-xs font-semibold transition-all ${
                  selectedTagId === tag.id
                    ? "bg-brand text-white shadow-xs"
                    : "hover:border-brand/40 hover:text-brand dark:hover:border-brand/40 dark:hover:text-brand border border-neutral-200/80 bg-white text-neutral-600 hover:bg-neutral-50 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                }`}
                style={
                  tag.color && selectedTagId === tag.id
                    ? { backgroundColor: tag.color }
                    : tag.color
                      ? { borderLeftColor: tag.color, borderLeftWidth: "3px" }
                      : undefined
                }
              >
                {tag.name}
              </button>
            ))}
          </div>
        )}

        {/* Filter Summary */}
        <div className="flex items-center justify-between px-1 text-xs text-neutral-500 dark:text-zinc-400">
          <div className="flex items-center gap-1.5">
            <Filter className="text-brand size-3.5" />
            <span>
              {totalCount > 0 ? (
                <span>
                  {totalCount} {t("sharing.exploreTitle").toLowerCase()}
                </span>
              ) : (
                <span>{t("sharing.feedTitle")}</span>
              )}
            </span>
          </div>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => refetch()}
            className="hover:text-brand h-7 text-[11px] text-neutral-500"
          >
            <RefreshCw className="mr-1 size-3" />
            <span>{t("sharing.refresh")}</span>
          </Button>
        </div>
      </div>

      {/* Feed Stream Content */}
      <div className="space-y-5">
        {/* Loading Initial State */}
        {isLoading && (
          <div className="space-y-4">
            {[1, 2, 3].map((i) => (
              <Card
                key={i}
                variant="default"
                className="animate-pulse space-y-4 rounded-2xl border border-neutral-200/80 p-6 dark:border-zinc-800 dark:bg-zinc-900"
              >
                <div className="flex items-center gap-3">
                  <div className="size-11 rounded-full bg-neutral-200 dark:bg-zinc-800" />
                  <div className="space-y-2">
                    <div className="h-4 w-32 rounded-md bg-neutral-200 dark:bg-zinc-800" />
                    <div className="h-3 w-20 rounded-md bg-neutral-200 dark:bg-zinc-800" />
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="h-5 w-3/4 rounded-md bg-neutral-200 dark:bg-zinc-800" />
                  <div className="h-4 w-full rounded-md bg-neutral-200 dark:bg-zinc-800" />
                  <div className="h-4 w-2/3 rounded-md bg-neutral-200 dark:bg-zinc-800" />
                </div>
              </Card>
            ))}
          </div>
        )}

        {/* Error State */}
        {isError && (
          <Card
            variant="default"
            className="rounded-2xl border border-rose-200 bg-rose-50/50 p-6 text-center dark:border-rose-900/60 dark:bg-rose-950/30"
          >
            <p className="text-sm font-semibold text-rose-700 dark:text-rose-300">
              {error?.message || "Failed to load feed articles."}
            </p>
            <Button
              variant="brand"
              size="sm"
              onClick={() => refetch()}
              className="mt-3 font-semibold"
            >
              {t("sharing.refresh")}
            </Button>
          </Card>
        )}

        {/* Empty State */}
        {!isLoading && !isError && allPosts.length === 0 && (
          <Card
            variant="default"
            className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-300 bg-neutral-50/50 p-12 text-center dark:border-zinc-800 dark:bg-zinc-900/50"
          >
            <div className="bg-brand/10 text-brand flex size-14 items-center justify-center rounded-2xl">
              <BookOpen className="size-7" />
            </div>
            <h4 className="mt-4 text-base font-bold text-neutral-900 dark:text-zinc-100">
              {t("sharing.emptyContents")}
            </h4>
            <p className="mt-1 max-w-sm text-xs leading-relaxed text-neutral-500 dark:text-zinc-400">
              {t("sharing.emptyContentsDesc")}
            </p>
          </Card>
        )}

        {/* List of Feed Post Cards */}
        {allPosts.map((post) => (
          <FeedPostCard key={post.id} content={post} />
        ))}

        {/* Infinite Scroll Sentinel */}
        <div ref={sentinelRef} className="h-1 w-full" />

        {/* Fetching Next Page Loader */}
        {isFetchingNextPage && (
          <div className="flex items-center justify-center gap-2.5 py-6">
            <Loader2 className="text-brand size-5 animate-spin" />
            <span className="text-xs font-semibold text-neutral-600 dark:text-zinc-400">
              {t("sharing.loadingMore")}
            </span>
          </div>
        )}

        {/* All Caught Up Indicator */}
        {!isLoading && !hasNextPage && allPosts.length > 0 && (
          <div className="flex flex-col items-center justify-center gap-2 py-8 text-center">
            <div className="flex size-9 items-center justify-center rounded-full bg-emerald-100 text-emerald-600 dark:bg-emerald-950/60 dark:text-emerald-400">
              <CheckCircle className="size-5" />
            </div>
            <p className="text-xs font-semibold text-neutral-600 dark:text-zinc-400">
              {t("sharing.allCaughtUp")}
            </p>
            <span className="text-[11px] text-neutral-400 dark:text-zinc-500">
              <Sparkles className="text-brand mr-1 inline-block size-3" />
              GDSC Sharing Platform
            </span>
          </div>
        )}
      </div>
    </div>
  );
}
