"use client";

import React, { useState } from "react";
import Link from "next/link";
import {
  ContentFilterBar,
  ContentList,
  ContentFormDialog,
  useContentsQuery,
  useTagsQuery,
} from "@/features/sharing";
import { useTranslation } from "@/core/i18n/i18n.context";
import { Button } from "@/components/ui/button";
import { Plus, FolderKanban, Sparkles } from "lucide-react";

export default function SharingPage() {
  const { t } = useTranslation();

  const [search, setSearch] = useState("");
  const [selectedTagId, setSelectedTagId] = useState<string | undefined>();
  const [sort, setSort] = useState<"newest" | "oldest" | "title">("newest");
  const [page, setPage] = useState(1);
  const [isCreateOpen, setIsCreateOpen] = useState(false);

  // Queries
  const { data: tags = [] } = useTagsQuery();
  const { data: contentsData, isLoading } = useContentsQuery({
    search: search.trim() || undefined,
    tagId: selectedTagId,
    sort,
    page,
    pageSize: 12,
  });

  return (
    <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Hero / Header banner */}
      <div className="relative overflow-hidden rounded-3xl border border-neutral-200/80 bg-linear-to-r from-blue-500/10 via-indigo-500/10 to-pink-500/10 p-8 shadow-xs dark:border-zinc-800 dark:from-blue-950/20 dark:via-indigo-950/20 dark:to-pink-950/20">
        <div className="relative z-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="max-w-2xl space-y-2">
            <div className="inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50/80 px-3 py-1 text-xs font-semibold text-blue-700 shadow-2xs dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
              <Sparkles className="size-3.5" />
              <span>GDSC Knowledge Hub</span>
            </div>
            <h1 className="text-3xl font-extrabold tracking-tight text-neutral-900 sm:text-4xl dark:text-white">
              {t("sharing.title")}
            </h1>
            <p className="text-sm leading-relaxed text-neutral-600 dark:text-zinc-300">
              {t("sharing.subtitle")}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {/* My Content button */}
            <Link href="/sharing/mine">
              <Button variant="outline" className="shadow-2xs">
                <FolderKanban className="mr-1.5 size-4" />
                <span>{t("sharing.myContents")}</span>
              </Button>
            </Link>

            {/* Create Content button */}
            <Button
              variant="brand"
              onClick={() => setIsCreateOpen(true)}
              className="font-semibold shadow-2xs"
            >
              <Plus className="mr-1.5 size-4" />
              <span>{t("sharing.createContent")}</span>
            </Button>
          </div>
        </div>
      </div>

      {/* Filter and Search Bar */}
      <ContentFilterBar
        search={search}
        onSearchChange={(val) => {
          setSearch(val);
          setPage(1);
        }}
        selectedTagId={selectedTagId}
        onTagChange={(tagId) => {
          setSelectedTagId(tagId);
          setPage(1);
        }}
        tags={tags}
        sort={sort}
        onSortChange={(newSort) => {
          setSort(newSort);
          setPage(1);
        }}
      />

      {/* Grid of Articles */}
      <ContentList
        items={contentsData?.items}
        isLoading={isLoading}
        totalCount={contentsData?.totalCount}
        page={page}
        pageSize={12}
        onPageChange={setPage}
      />

      {/* Create Content Dialog */}
      <ContentFormDialog
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
      />
    </div>
  );
}
