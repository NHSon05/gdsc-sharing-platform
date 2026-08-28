"use client";

import React from "react";
import Link from "next/link";
import { ThemeToggle } from "@/components/ThemeToggle";
import { LanguageToggle } from "@/components/LanguageToggle";
import { OrbitingIcons } from "@/components/OrbitingIcons";
import { Button } from "@/components/ui/button";
import { ArrowRight } from "lucide-react";
import { useTranslation } from "@/core/i18n/i18n.context";
import { useSessionStore } from "@/core/session/session.store";
import { selectIsAuthenticated } from "@/core/session/session.selectors";
import { useCurrentUserQuery } from "@/features/auth/hooks/use-current-user-query";
import { useLogoutMutation } from "@/features/auth/hooks/use-logout-mutation";
import { LogOut, User } from "lucide-react";

const emptySubscribe = () => () => {};

export default function Home() {
  const { t } = useTranslation();
  const isClient = React.useSyncExternalStore(
    emptySubscribe,
    () => true,
    () => false
  );
  const isAuthenticated = useSessionStore(selectIsAuthenticated);
  const { data: currentUser } = useCurrentUserQuery();
  const logoutMutation = useLogoutMutation();

  return (
    <div className="relative flex min-h-dvh w-full flex-col items-center justify-start overflow-x-hidden bg-white px-4 pt-16 pb-20 font-sans text-neutral-900 transition-colors duration-300 select-none md:pt-24 dark:bg-[#09090b] dark:text-zinc-100">
      {/* Top Navbar with Language and Theme Toggles */}
      <div className="absolute top-6 right-6 z-50 flex items-center gap-2.5 md:top-8 md:right-10">
        <LanguageToggle />
        <ThemeToggle />
      </div>

      {/* Top Hero Content */}
      <div className="max-w-8xl z-10 mx-auto flex flex-col items-center text-center">
        {/* Main Title */}
        <h1 className="text-brand text-5xl leading-[1.08] font-bold tracking-tight sm:text-6xl md:text-[68px]">
          {t("hero.title")}
        </h1>

        {/* Subtitle */}
        <p className="mt-5 max-w-2xl text-base leading-relaxed font-normal text-neutral-600 sm:text-lg md:text-[19px] dark:text-zinc-400">
          {t("hero.subtitle")}
        </p>

        {/* Action Buttons */}
        <div className="mt-8 flex flex-row items-center justify-center gap-4">
          {/* View Detail Button */}
          <Link href="#tracks">
            <Button
              variant="elevated"
              size="lg"
              rightIcon={<ArrowRight className="size-4" />}
              className="text-brand hover:text-brand-hover font-semibold"
            >
              {t("common.viewDetail")}
            </Button>
          </Link>

          {/* Login or User Profile & Logout Button */}
          {isClient && isAuthenticated ? (
            <div className="flex items-center gap-3">
              <Link href="/profile">
                <Button
                  variant="brand"
                  size="lg"
                  leftIcon={<User className="size-4" />}
                  className="font-semibold"
                >
                  {currentUser?.displayName || "Profile"}
                </Button>
              </Link>
              <Button
                variant="outline"
                size="lg"
                onClick={() => logoutMutation.mutate()}
                disabled={logoutMutation.isPending}
                leftIcon={<LogOut className="size-4" />}
                className="font-semibold text-rose-600 hover:bg-rose-50 hover:text-rose-700 dark:text-rose-400 dark:hover:bg-rose-950/30"
              >
                Logout
              </Button>
            </div>
          ) : (
            <Link href="/login">
              <Button variant="brand" size="lg" className="font-semibold">
                {t("common.login")}
              </Button>
            </Link>
          )}
        </div>

        {/* Social Proof */}
        <div className="mt-8 flex items-center gap-3 text-xs text-neutral-700 md:text-sm dark:text-zinc-300">
          {/* Avatar stack */}
          <div className="flex items-center -space-x-1.5">
            {[1, 2, 3, 4].map((num) => (
              <div
                key={num}
                className="flex h-6 w-6 items-center justify-center rounded-full border-2 border-white bg-neutral-300 text-[10px] font-medium text-white shadow-xs dark:border-[#09090b] dark:bg-zinc-700"
              >
                {num}
              </div>
            ))}
          </div>

          {/* Star rating */}
          <div className="flex items-center gap-0.5 text-amber-400">
            {[...Array(5)].map((_, i) => (
              <svg
                key={i}
                className="h-3.5 w-3.5 fill-current"
                viewBox="0 0 24 24"
              >
                <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
              </svg>
            ))}
          </div>

          {/* Trust text */}
          <span className="text-xs font-normal text-neutral-600 md:text-[13px] dark:text-zinc-400">
            {t("hero.trustedBy")}
          </span>
        </div>
      </div>

      {/* Hero Visual Section */}
      <div className="relative mx-auto mt-12 flex h-90 w-full max-w-4xl items-center justify-center sm:h-100 md:mt-20">
        {/* Soft Radial Ambient Brand Glow with Breathing Pulse */}
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
          <div className="from-brand/20 dark:from-brand/15 animate-pulse-glow h-85 w-125 rounded-full bg-linear-to-r via-sky-400/25 to-blue-500/20 opacity-80 blur-3xl dark:via-sky-500/20 dark:to-blue-600/15" />
        </div>

        {/* Central Vertical Neon Brand Light Ray with Beam Pulse */}
        <div className="via-brand animate-beam-pulse pointer-events-none absolute top-1/2 left-1/2 h-64 w-0.5 -translate-x-1/2 -translate-y-1/2 bg-linear-to-b from-transparent to-transparent opacity-80">
          <div className="bg-brand absolute inset-0 opacity-90 blur-[2px]" />
        </div>

        {/* Dynamic Sparkling Particle Dots */}
        <div className="bg-brand animate-twinkle-1 pointer-events-none absolute top-16 left-[48%] h-1.5 w-1.5 rounded-full blur-[0.5px]" />
        <div className="animate-twinkle-2 pointer-events-none absolute top-28 left-[53%] h-1.5 w-1.5 rounded-full bg-sky-400 blur-[0.5px]" />
        <div className="animate-twinkle-3 pointer-events-none absolute bottom-24 left-[46%] h-1.5 w-1.5 rounded-full bg-blue-500 blur-[0.5px]" />
        <div className="bg-brand animate-twinkle-4 pointer-events-none absolute bottom-16 left-[54%] h-1.5 w-1.5 rounded-full blur-[0.5px]" />

        {/* Left Side: Orbiting Track Icons */}
        <div className="relative hidden h-full w-full max-w-md items-center justify-center sm:flex">
          <OrbitingIcons />
        </div>

        {/* Right Side: Stacked Glass Metric Cards with Floating Physics */}
        <div className="z-10 flex w-full flex-col gap-4 px-4 sm:mr-4 sm:ml-auto sm:w-95 sm:px-0 md:w-102.5">
          {/* Card 1: API Response Time Optimization (Float Slow) */}
          <div className="hover:border-brand/60 dark:hover:border-brand/50 animate-float-slow rounded-2xl border border-neutral-100 bg-white/95 p-5 shadow-[0_15px_35px_-8px_rgba(0,0,0,0.07)] backdrop-blur-md transition-all duration-300 hover:shadow-[0_20px_40px_-10px_rgba(66,133,244,0.18)] dark:border-zinc-800/90 dark:bg-zinc-900/85 dark:shadow-[0_15px_35px_-8px_rgba(0,0,0,0.45)]">
            <div className="flex items-center gap-3">
              <div className="bg-brand-muted text-brand flex h-7 w-7 items-center justify-center rounded-lg">
                <svg className="fill-brand h-4 w-4" viewBox="0 0 24 24">
                  <path d="M13 2L3 14h8l-1 8 11-13h-8l1-7z" />
                </svg>
              </div>
              <h3 className="text-sm font-semibold tracking-tight text-neutral-900 md:text-base dark:text-zinc-100">
                {t("metrics.apiOptimizationTitle")}
              </h3>
            </div>
            <p className="mt-2 pl-10 text-xs leading-relaxed font-normal text-neutral-500 md:text-[13px] dark:text-zinc-400">
              {t("metrics.apiOptimizationDesc")}
            </p>
          </div>

          {/* Card 2: System Health Check (Float Delayed + Live Ping) */}
          <div className="hover:border-brand/60 dark:hover:border-brand/50 animate-float-delayed rounded-2xl border border-neutral-100 bg-white/95 p-5 shadow-[0_15px_35px_-8px_rgba(0,0,0,0.07)] backdrop-blur-md transition-all duration-300 hover:shadow-[0_20px_40px_-10px_rgba(66,133,244,0.18)] dark:border-zinc-800/90 dark:bg-zinc-900/85 dark:shadow-[0_15px_35px_-8px_rgba(0,0,0,0.45)]">
            <div className="flex items-center gap-3">
              <div className="bg-brand-muted text-brand flex h-7 w-7 items-center justify-center rounded-lg">
                <svg
                  className="stroke-brand h-4 w-4 fill-none"
                  viewBox="0 0 24 24"
                  strokeWidth="2.2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <circle cx="12" cy="12" r="9" />
                  <polyline points="9 12 11.5 14.5 15.5 9.5" />
                </svg>
              </div>
              <h3 className="text-sm font-semibold tracking-tight text-neutral-900 md:text-base dark:text-zinc-100">
                {t("metrics.systemHealthTitle")}
              </h3>
            </div>
            <ul className="mt-2 space-y-1.5 pl-10 text-xs font-normal text-neutral-500 md:text-[13px] dark:text-zinc-400">
              <li className="flex items-center gap-2.5">
                {/* Live Ping Indicator */}
                <span className="relative flex h-2 w-2">
                  <span className="bg-brand absolute inline-flex h-full w-full animate-ping rounded-full opacity-75"></span>
                  <span className="bg-brand relative inline-flex h-2 w-2 rounded-full"></span>
                </span>
                <span>{t("metrics.allServicesOperational")}</span>
              </li>
              <li className="flex items-center gap-2.5">
                <span className="bg-brand inline-block h-2 w-2 rounded-full" />
                <span>{t("metrics.uptime")}</span>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Domain & Tracks Section */}
      <section
        id="tracks"
        className="z-10 mx-auto mt-28 flex w-full max-w-6xl flex-col items-center px-4 md:mt-36"
      >
        {/* Section Heading */}
        <div className="mb-12 text-center">
          <h2 className="text-3xl font-bold tracking-tight text-neutral-900 sm:text-4xl dark:text-zinc-50">
            {t("tracks.heading")}
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-sm text-neutral-500 md:text-base dark:text-zinc-400">
            {t("tracks.subheading")}
          </p>
        </div>

        {/* Bento / Grid Showcase */}
        <div className="grid w-full grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {/* 1. Frontend */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <rect width="18" height="18" x="3" y="3" rx="2" />
                  <path d="M3 9h18" />
                  <path d="M9 21V9" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.frontend.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.frontend.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Next.js 16
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                React 19
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                UI/UX
              </span>
            </div>
          </div>

          {/* 2. Backend */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <ellipse cx="12" cy="5" rx="9" ry="3" />
                  <path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5" />
                  <path d="M3 12c0 1.66 4 3 9 3s9-1.34 9-3" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.backend.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.backend.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                ASP.NET Core
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                PostgreSQL
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Microservices
              </span>
            </div>
          </div>

          {/* 3. AI & Machine Learning */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M12 2v4" />
                  <path d="M12 18v4" />
                  <path d="M4.93 4.93l2.83 2.83" />
                  <path d="M16.24 16.24l2.83 2.83" />
                  <path d="M2 12h4" />
                  <path d="M18 12h4" />
                  <path d="M4.93 19.07l2.83-2.83" />
                  <path d="M16.24 7.76l2.83-2.83" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.ai.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.ai.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                LLMs
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                RAG Agents
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                PyTorch
              </span>
            </div>
          </div>

          {/* 4. DevOps & Cloud */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M17.5 19H9a7 7 0 1 1 6.71-9h1.79a4.5 4.5 0 1 1 0 9Z" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.devops.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.devops.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Docker
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Kubernetes
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                CI/CD
              </span>
            </div>
          </div>

          {/* 5. Business & Product */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M3 3v18h18" />
                  <path d="m19 9-5 5-4-4-3 3" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.business.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.business.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Product Mgmt
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                MVP
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Pitching
              </span>
            </div>
          </div>

          {/* 6. Marketing & DevRel */}
          <div className="group hover:border-brand/60 dark:hover:border-brand/60 relative flex flex-col justify-between rounded-3xl bg-white p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] backdrop-blur-md transition-all duration-300 hover:-translate-y-1.5 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] dark:bg-zinc-900/80 dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)]">
            <div>
              <div className="bg-brand-muted border-brand-border text-brand group-hover:bg-brand flex h-12 w-12 items-center justify-center rounded-2xl border transition-all duration-300 group-hover:scale-110 group-hover:text-white">
                <svg
                  className="h-6 w-6 fill-none stroke-current"
                  viewBox="0 0 24 24"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="m3 11 18-5v12L3 14v-3z" />
                  <path d="M11.6 16.8a3 3 0 1 1-5.8-1.6" />
                </svg>
              </div>
              <h3 className="group-hover:text-brand mt-5 text-xl font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-50">
                {t("tracks.marketing.title")}
              </h3>
              <p className="mt-2.5 text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("tracks.marketing.desc")}
              </p>
            </div>
            <div className="mt-6 flex flex-wrap gap-2 border-t border-neutral-100 pt-5 dark:border-zinc-800/80">
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                DevRel
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Tech Branding
              </span>
              <span className="rounded-md bg-neutral-100 px-2.5 py-1 font-mono text-xs text-neutral-700 dark:bg-zinc-800 dark:text-zinc-300">
                Growth
              </span>
            </div>
          </div>
        </div>
      </section>

      {/* Footer Note */}
      <footer className="mx-auto mt-28 w-full max-w-6xl border-t border-neutral-100 pt-8 text-center text-xs text-neutral-400 dark:border-zinc-900 dark:text-zinc-600">
        {t("common.footerNote")}
      </footer>
    </div>
  );
}
