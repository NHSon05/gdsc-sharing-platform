"use client";

import React from "react";
import Link from "next/link";
import { Search, ChevronDown, X } from "lucide-react";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface LearningSubNavProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  className?: string;
}

export function LearningSubNav({
  searchTerm,
  onSearchChange,
  className = "",
}: LearningSubNavProps) {
  const { t } = useTranslation();

  return (
    <header
      className={`border-b border-neutral-200/80 bg-white/95 backdrop-blur-md dark:border-zinc-800/80 dark:bg-[#0C0C0E]/95 ${className}`}
    >
      <div className="mx-auto flex max-w-7xl flex-col gap-3 px-4 py-2.5 sm:flex-row sm:items-center sm:justify-between sm:px-6 lg:px-8">
        {/* Navigation links matching screenshot */}
        <nav className="flex items-center gap-6 text-sm font-semibold">
          <Link
            href="/"
            className="text-neutral-900 transition-colors hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
          >
            {t("learning.home")}
          </Link>

          {/* Active "Luyện tập" dropdown */}
          <div className="border-brand text-brand dark:text-brand-hover relative flex items-center gap-1 border-b-2 pb-0.5 font-bold">
            <span>{t("learning.practice")}</span>
            <ChevronDown className="size-4" />
          </div>

          <div className="flex items-center gap-1.5 text-neutral-900 transition-colors hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white">
            <span>{t("learning.quiz")}</span>
            <span className="py-0.2 rounded-full bg-blue-100 px-1.5 text-[9px] font-bold text-blue-600 dark:bg-blue-950/80 dark:text-blue-400">
              {t("learning.isNew")}
            </span>
          </div>

          <Link
            href="#interview-review"
            className="hidden text-neutral-900 transition-colors hover:text-neutral-900 md:inline-block dark:text-zinc-400 dark:hover:text-white"
          >
            {t("learning.interviewReviews")}
          </Link>

          <Link
            href="#community"
            className="hidden text-neutral-900 transition-colors hover:text-neutral-900 md:inline-block dark:text-zinc-400 dark:hover:text-white"
          >
            {t("learning.community")}
          </Link>
        </nav>

        {/* Centered / Right Search Bar */}
        <div className="relative w-full sm:w-80">
          <Search className="absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-neutral-600 dark:text-zinc-500" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder={t("learning.searchPlaceholder")}
            className="focus:border-brand w-full rounded-full border border-neutral-600 bg-neutral-50 py-2 pr-8 pl-9 text-sm text-neutral-900 transition-all placeholder:text-neutral-400 focus:bg-white focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100 dark:focus:bg-zinc-900"
          />
          {searchTerm && (
            <button
              type="button"
              onClick={() => onSearchChange("")}
              className="absolute top-1/2 right-3 -translate-y-1/2 text-neutral-600 hover:text-neutral-600 dark:text-zinc-500 dark:hover:text-zinc-300"
            >
              <X className="size-3.5" />
            </button>
          )}
        </div>
      </div>
    </header>
  );
}
