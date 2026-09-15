"use client";

import React from "react";
import Link from "next/link";
import { MyContentsManager } from "@/features/sharing";
import { useTranslation } from "@/core/i18n/i18n.context";
import { ArrowLeft, FolderKanban } from "lucide-react";

export default function MyContentsPage() {
  const { t } = useTranslation();

  return (
    <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Navigation Breadcrumb */}
      <div>
        <Link
          href="/sharing"
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-neutral-500 transition-colors hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
        >
          <ArrowLeft className="size-4" />
          <span>Back to all content</span>
        </Link>
      </div>

      {/* Header Info */}
      <div className="space-y-1 border-b border-neutral-100 pb-4 dark:border-zinc-800">
        <div className="flex items-center gap-2">
          <div className="flex size-9 items-center justify-center rounded-xl bg-blue-50 text-blue-600 dark:bg-blue-950/50 dark:text-blue-400">
            <FolderKanban className="size-5" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
            {t("sharing.myContents")}
          </h1>
        </div>
        <p className="text-xs text-neutral-500 dark:text-zinc-400">
          Manage your drafts, pending review submissions, published articles,
          and feedback.
        </p>
      </div>

      {/* Tabbed Workspace Component */}
      <MyContentsManager />
    </div>
  );
}
