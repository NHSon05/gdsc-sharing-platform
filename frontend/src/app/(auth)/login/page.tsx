"use client";

import React from "react";
import Image from "next/image";
import Link from "next/link";
import bgClub from "@/assets/images/background-clb.png";
import { LoginForm } from "@/components/auth/login-form";
import { ThemeToggle } from "@/components/ThemeToggle";
import { LanguageToggle } from "@/components/LanguageToggle";
import { useTranslation } from "@/core/i18n/i18n.context";

export default function LoginPage() {
  const { t } = useTranslation();

  return (
    <div className="selection:bg-brand selection:text-brand-foreground relative flex min-h-dvh w-full flex-col items-center justify-center overflow-x-hidden p-4 transition-colors duration-300 sm:p-6 md:p-10">
      {/* Full-Page Background Image */}
      <div className="absolute inset-0 -z-20 overflow-hidden">
        <Image
          src={bgClub}
          alt="GDSC Club Background"
          fill
          priority
          quality={100}
          className="object-cover object-center transition-all duration-700 dark:opacity-40"
        />
      </div>

      {/* Top Navigation Bar with Back to Home, Language and Theme Toggles */}
      <div className="absolute top-6 left-6 z-50 md:top-8 md:left-10">
        <Link
          href="/"
          className="hover:border-brand/60 dark:hover:border-brand/50 group flex items-center gap-2 rounded-full border border-neutral-200/80 bg-white/80 px-3.5 py-1.5 text-xs font-medium text-neutral-700 shadow-xs backdrop-blur-md transition-all hover:text-neutral-900 md:text-sm dark:border-zinc-800 dark:bg-zinc-900/80 dark:text-zinc-300 dark:hover:text-white"
        >
          <svg
            className="h-4 w-4 fill-none stroke-current transition-transform group-hover:-translate-x-0.5"
            viewBox="0 0 24 24"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="m15 18-6-6 6-6" />
          </svg>
          <span>{t("common.backToHome")}</span>
        </Link>
      </div>

      <div className="absolute top-6 right-6 z-50 flex items-center gap-2.5 md:top-8 md:right-10">
        <LanguageToggle />
        <ThemeToggle />
      </div>

      {/* Centered Liquid Glass Login Form */}
      <div className="relative z-10 my-auto w-full max-w-md">
        <React.Suspense fallback={<div className="h-[400px] w-full animate-pulse rounded-3xl bg-white/10" />}>
          <LoginForm />
        </React.Suspense>
      </div>
    </div>
  );
}
