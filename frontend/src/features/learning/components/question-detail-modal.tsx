"use client";

import React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { QuestionAnswerView } from "./question-answer-view";
import { useInterviewQuestionDetailQuery } from "../hooks/use-interview-question-detail-query";
import { Loader2, Copy, Check } from "lucide-react";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface QuestionDetailModalProps {
  questionId: string | null;
  questionTitle?: string;
  level?: string;
  indexNumber?: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function QuestionDetailModal({
  questionId,
  questionTitle,
  level = "basic",
  indexNumber,
  open,
  onOpenChange,
}: QuestionDetailModalProps) {
  const [copied, setCopied] = React.useState(false);
  const { t } = useTranslation();

  const { data, isLoading } = useInterviewQuestionDetailQuery(
    questionId || "",
    Boolean(questionId) && open
  );

  const handleCopy = () => {
    if (questionTitle) {
      navigator.clipboard.writeText(questionTitle);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const question = data?.data?.question || questionTitle || "";
  const answer = data?.data?.answer;

  const renderLevelBadge = (lvl: string) => {
    switch (lvl?.toLowerCase()) {
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
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[88vh] max-w-3xl overflow-y-auto rounded-3xl p-6 sm:p-8">
        <DialogHeader className="space-y-3 pb-2 text-left">
          <div className="flex items-center gap-2">
            {indexNumber !== undefined && (
              <span className="text-xs font-mono font-bold text-neutral-400 dark:text-zinc-500">
                #{indexNumber}
              </span>
            )}
            {renderLevelBadge(level)}
          </div>

          <div className="flex items-start justify-between gap-4">
            <DialogTitle className="text-base font-bold leading-snug text-neutral-900 sm:text-lg dark:text-white">
              {question}
            </DialogTitle>

            <button
              type="button"
              onClick={handleCopy}
              className="mt-0.5 flex size-7 shrink-0 cursor-pointer items-center justify-center rounded-lg text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-700 dark:text-zinc-500 dark:hover:bg-zinc-800 dark:hover:text-zinc-200"
              title={t("learning.copyQuestion")}
            >
              {copied ? (
                <Check className="size-4 text-emerald-600 dark:text-emerald-400" />
              ) : (
                <Copy className="size-4" />
              )}
            </button>
          </div>
        </DialogHeader>

        {isLoading ? (
          <div className="flex min-h-48 flex-col items-center justify-center">
            <Loader2 className="size-7 animate-spin text-brand" />
            <p className="mt-2 text-xs font-semibold text-neutral-500 dark:text-zinc-400">
              {t("learning.loadingDetail")}
            </p>
          </div>
        ) : (
          <div className="mt-4 border-t border-neutral-100 pt-4 dark:border-zinc-800">
            <QuestionAnswerView answer={answer} />
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
