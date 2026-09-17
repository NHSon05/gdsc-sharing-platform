"use client";

import React, { useState } from "react";
import {
  Copy,
  Check,
  Bookmark,
  GraduationCap,
  Maximize2,
  ExternalLink,
  ChevronDown,
  ChevronUp,
  Loader2,
} from "lucide-react";
import type { InterviewQuestionSummary } from "../types/learning.types";
import { useInterviewQuestionDetailQuery } from "../hooks/use-interview-question-detail-query";
import { QuestionAnswerView } from "./question-answer-view";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface QuestionCardProps {
  item: InterviewQuestionSummary;
  indexNumber: number;
  onOpenModal: (item: InterviewQuestionSummary, indexNumber: number) => void;
  className?: string;
}

export function QuestionCard({
  item,
  indexNumber,
  onOpenModal,
  className = "",
}: QuestionCardProps) {
  const [copied, setCopied] = useState(false);
  const [bookmarked, setBookmarked] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);
  const { t } = useTranslation();

  // Fetch details on-demand when expanded
  const { data: detailData, isLoading } = useInterviewQuestionDetailQuery(
    item.id,
    isExpanded
  );

  const handleCopy = (e: React.MouseEvent) => {
    e.stopPropagation();
    navigator.clipboard.writeText(item.question);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleToggleExpand = () => {
    setIsExpanded(!isExpanded);
  };

  const renderLevelBadge = (level: string) => {
    switch (level?.toLowerCase()) {
      case "basic":
        return (
          <span className="rounded-md border border-blue-500/30 bg-blue-50 px-2 py-0.5 text-[10px] font-bold text-blue-600 dark:border-blue-500/30 dark:bg-blue-950/40 dark:text-blue-400">
            {t("learning.basicLevelBadge")}
          </span>
        );
      case "intermediate":
        return (
          <span className="rounded-md border border-amber-500/30 bg-amber-50 px-2 py-0.5 text-[10px] font-bold text-amber-600 dark:border-amber-500/30 dark:bg-amber-950/40 dark:text-amber-400">
            {t("learning.intermediateLevelBadge")}
          </span>
        );
      case "advanced":
        return (
          <span className="rounded-md border border-rose-500/30 bg-rose-50 px-2 py-0.5 text-[10px] font-bold text-rose-600 dark:border-rose-500/30 dark:bg-rose-950/40 dark:text-rose-400">
            {t("learning.advancedLevelBadge")}
          </span>
        );
      default:
        return (
          <span className="rounded-md border border-neutral-200 bg-neutral-100 px-2 py-0.5 text-[10px] font-bold text-neutral-600 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300">
            {t("learning.basicLevelBadge")}
          </span>
        );
    }
  };

  return (
    <div
      className={`group rounded-2xl border border-neutral-200/90 bg-white transition-all duration-200 hover:border-neutral-300 hover:shadow-2xs dark:border-zinc-800/90 dark:bg-[#0C0C0E] dark:hover:border-zinc-700 ${
        isExpanded ? "ring-1 ring-neutral-200 dark:ring-zinc-800" : ""
      } ${className}`}
    >
      {/* Header Row */}
      <div
        onClick={handleToggleExpand}
        className="flex cursor-pointer flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:gap-6 sm:px-6 sm:py-4.5"
      >
        {/* Left: Index number + Question title + Copy icon */}
        <div className="flex min-w-0 flex-1 items-baseline gap-3.5 sm:items-center">
          <span className="shrink-0 font-mono text-sm font-bold text-neutral-400 dark:text-zinc-500">
            #{indexNumber}
          </span>

          <div className="flex min-w-0 flex-1 items-center gap-2">
            <h3 className="text-sm font-bold leading-normal text-neutral-900 transition-colors group-hover:text-blue-600 dark:text-zinc-100 dark:group-hover:text-blue-400">
              {item.question}
            </h3>

            <button
              type="button"
              onClick={handleCopy}
              className="inline-flex size-6 shrink-0 cursor-pointer items-center justify-center rounded-md text-neutral-400 opacity-70 transition-all group-hover:opacity-100 hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
              title={t("learning.copyQuestion")}
            >
              {copied ? (
                <Check className="size-3.5 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <Copy className="size-3.5" />
              )}
            </button>
          </div>
        </div>

        {/* Right: Difficulty badge + Action icons matching screenshot */}
        <div className="flex shrink-0 items-center gap-1.5 self-end sm:self-center">
          {/* Level Badge */}
          <div className="mr-1">
            {renderLevelBadge(item.level)}
          </div>

          {/* Bookmark Button */}
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setBookmarked(!bookmarked);
            }}
            title={
              bookmarked ? t("learning.saved") : t("learning.saveQuestion")
            }
            className="flex size-7.5 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <Bookmark
              className={`size-3.5 ${
                bookmarked
                  ? "fill-amber-500 text-amber-500 dark:fill-amber-400 dark:text-amber-400"
                  : ""
              }`}
            />
          </button>

          {/* Practice / Quiz Cap Button */}
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onOpenModal(item, indexNumber);
            }}
            title={t("learning.practiceQuiz")}
            className="flex size-7.5 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <GraduationCap className="size-4" />
          </button>

          {/* Maximize Dialog Button */}
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onOpenModal(item, indexNumber);
            }}
            title={t("learning.viewFullscreen")}
            className="flex size-7.5 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <Maximize2 className="size-3.5" />
          </button>

          {/* External Link Button */}
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onOpenModal(item, indexNumber);
            }}
            title={t("learning.questionDetail")}
            className="flex size-7.5 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
          >
            <ExternalLink className="size-3.5" />
          </button>

          {/* Expand indicator arrow */}
          <div className="flex size-7.5 items-center justify-center text-neutral-400 dark:text-zinc-500">
            {isExpanded ? (
              <ChevronUp className="size-4" />
            ) : (
              <ChevronDown className="size-4" />
            )}
          </div>
        </div>
      </div>

      {/* Accordion Expandable Answer Section */}
      {isExpanded && (
        <div className="border-t border-neutral-100 bg-neutral-50/40 p-4.5 transition-all sm:px-6 sm:py-5 dark:border-zinc-800/80 dark:bg-zinc-950/40">
          {isLoading ? (
            <div className="flex min-h-24 items-center justify-center gap-2 text-neutral-500 dark:text-zinc-400">
              <Loader2 className="text-brand size-4 animate-spin" />
              <span className="text-xs">{t("learning.loadingAnswer")}</span>
            </div>
          ) : (
            <QuestionAnswerView answer={detailData?.data?.answer} />
          )}
        </div>
      )}
    </div>
  );
}
