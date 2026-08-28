"use client";

import React from "react";
import { useTranslation } from "@/core/i18n/i18n.context";

export function LanguageToggle() {
  const { locale, setLocale } = useTranslation();

  return (
    <div className="inline-flex items-center rounded-full border border-neutral-200/80 bg-white/80 p-0.5 shadow-[0_4px_16px_rgba(0,0,0,0.04),inset_0_1px_0_rgba(255,255,255,0.7)] backdrop-blur-md transition-all dark:border-zinc-800 dark:bg-zinc-900/80 dark:shadow-[0_4px_16px_rgba(0,0,0,0.4),inset_0_1px_0_rgba(255,255,255,0.1)]">
      {/* English Pill */}
      <button
        type="button"
        onClick={() => setLocale("en")}
        aria-label="Switch to English"
        className={`relative cursor-pointer rounded-full px-2.5 py-1 text-xs font-semibold transition-all duration-200 ${
          locale === "en"
            ? "bg-brand text-brand-foreground shadow-[0_2px_8px_var(--brand-glow)]"
            : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
        }`}
      >
        EN
      </button>

      {/* Vietnamese Pill */}
      <button
        type="button"
        onClick={() => setLocale("vi")}
        aria-label="Chuyển sang Tiếng Việt"
        className={`relative cursor-pointer rounded-full px-2.5 py-1 text-xs font-semibold transition-all duration-200 ${
          locale === "vi"
            ? "bg-brand text-brand-foreground shadow-[0_2px_8px_var(--brand-glow)]"
            : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
        }`}
      >
        VI
      </button>
    </div>
  );
}
