"use client";

import React, { useState } from "react";
import { Check, Copy, Terminal, BookOpen, Layers } from "lucide-react";
import type { AnswerData } from "../types/learning.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface QuestionAnswerViewProps {
  answer: AnswerData | null | undefined;
  className?: string;
}

export function QuestionAnswerView({
  answer,
  className = "",
}: QuestionAnswerViewProps) {
  const [copiedCodeIndex, setCopiedCodeIndex] = useState<number | null>(null);
  const { t } = useTranslation();

  if (!answer) {
    return (
      <div className="rounded-2xl border border-dashed border-neutral-200 p-6 text-center text-sm text-neutral-600 dark:border-zinc-800 dark:text-zinc-400">
        {t("learning.noAnswerYet")}
      </div>
    );
  }

  const handleCopyCode = (code: string, index: number) => {
    navigator.clipboard.writeText(code);
    setCopiedCodeIndex(index);
    setTimeout(() => setCopiedCodeIndex(null), 2000);
  };

  const hasSummary = answer.summary && answer.summary.length > 0;
  const hasDetails = answer.details && answer.details.length > 0;
  const hasCode = answer.codeExamples && answer.codeExamples.length > 0;
  const hasPoints = answer.points && answer.points.length > 0;

  return (
    <div className={`space-y-4 text-sm leading-relaxed text-neutral-800 dark:text-zinc-200 ${className}`}>
      {/* Summary Bullet Points */}
      {hasSummary && (
        <div className="space-y-2 rounded-2xl bg-blue-50/60 p-4 text-neutral-800 dark:bg-blue-950/30 dark:text-blue-200">
          <div className="flex items-center gap-1.5 font-bold text-blue-700 dark:text-blue-300">
            <BookOpen className="size-4" />
            <span>{t("learning.coreSummary")}</span>
          </div>
          <ul className="list-disc space-y-1.5 pl-4 text-sm">
            {answer.summary.map((point, i) => (
              <li key={i} className="leading-normal">
                {point}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Details Paragraphs */}
      {hasDetails && (
        <div className="space-y-2 pt-1">
          {answer.details.map((detail, i) => (
            <p key={i} className="text-sm leading-relaxed text-neutral-800 dark:text-zinc-200">
              {detail}
            </p>
          ))}
        </div>
      )}

      {/* Key Points if any */}
      {hasPoints && (
        <div className="space-y-1.5 pt-1">
          <div className="flex items-center gap-1.5 font-bold text-neutral-900 dark:text-white">
            <Layers className="size-4 text-brand" />
            <span>{t("learning.keyPoints")}</span>
          </div>
          <ul className="list-disc space-y-1 pl-4 text-sm text-neutral-800 dark:text-zinc-200">
            {answer.points?.map((pt, i) => (
              <li key={i}>{pt}</li>
            ))}
          </ul>
        </div>
      )}

      {/* Code Examples */}
      {hasCode && (
        <div className="space-y-3 pt-2">
          {answer.codeExamples?.map((item, idx) => (
            <div
              key={idx}
              className="overflow-hidden rounded-2xl border border-neutral-200/90 bg-[#1E1E24] shadow-xs dark:border-zinc-800"
            >
              {/* Code Header with language and copy button */}
              <div className="flex items-center justify-between border-b border-white/10 bg-black/20 px-4 py-2">
                <div className="flex items-center gap-2">
                  <Terminal className="size-3.5 text-neutral-400" />
                  <span className="text-[11px] font-bold uppercase tracking-wider text-neutral-300">
                    {item.language || "Code"}
                  </span>
                </div>

                <button
                  type="button"
                  onClick={() => handleCopyCode(item.code, idx)}
                  className="flex cursor-pointer items-center gap-1.5 rounded-md px-2 py-1 text-[10px] font-semibold text-neutral-300 transition-colors hover:bg-white/10 hover:text-white"
                >
                  {copiedCodeIndex === idx ? (
                    <>
                      <Check className="size-3 text-emerald-400" />
                      <span className="text-emerald-400">{t("learning.codeCopied")}</span>
                    </>
                  ) : (
                    <>
                      <Copy className="size-3" />
                      <span>{t("learning.copyCode")}</span>
                    </>
                  )}
                </button>
              </div>

              {/* Code Content */}
              <pre className="overflow-x-auto p-4 font-mono text-[12px] leading-relaxed text-zinc-100">
                <code>{item.code}</code>
              </pre>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
