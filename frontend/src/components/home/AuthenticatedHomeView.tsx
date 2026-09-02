"use client";

import React, { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { TextField } from "@/components/ui/input";
import { useTranslation } from "@/core/i18n/i18n.context";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import {
  BookOpen,
  Calendar,
  PlusCircle,
  Search,
  Sparkles,
  ArrowRight,
  Clock,
  MapPin,
  Code2,
  Cpu,
  Server,
  ChevronRight,
} from "lucide-react";

import { useCurrentUserQuery } from "@/features/auth/hooks/use-current-user-query";

interface AuthenticatedHomeViewProps {
  user?: CurrentUserDto | null;
}

export function AuthenticatedHomeView({
  user: initialUser,
}: AuthenticatedHomeViewProps) {
  const { t } = useTranslation();
  const { data: userQuery } = useCurrentUserQuery();
  const user = initialUser || userQuery;
  const [searchQuery, setSearchQuery] = useState("");

  const roadmaps = [
    {
      id: "frontend",
      title: "Frontend Engineering 2026",
      category: "Frontend & UI/UX",
      icon: <Code2 className="size-5 text-blue-500" />,
      progress: 68,
      totalTopics: 24,
      completedTopics: 16,
      badge: "In Progress",
      color:
        "from-blue-500/10 to-sky-500/10 border-blue-200 dark:border-blue-900/50",
    },
    {
      id: "backend",
      title: "Clean Architecture & ASP.NET Core",
      category: "Backend & System Design",
      icon: <Server className="size-5 text-emerald-500" />,
      progress: 42,
      totalTopics: 18,
      completedTopics: 8,
      badge: "In Progress",
      color:
        "from-emerald-500/10 to-teal-500/10 border-emerald-200 dark:border-emerald-900/50",
    },
    {
      id: "ai",
      title: "Generative AI & Agentic Workflows",
      category: "AI & Machine Learning",
      icon: <Cpu className="size-5 text-purple-500" />,
      progress: 25,
      totalTopics: 12,
      completedTopics: 3,
      badge: "New",
      color:
        "from-purple-500/10 to-pink-500/10 border-purple-200 dark:border-purple-900/50",
    },
  ];

  const upcomingSessions = [
    {
      id: "1",
      title:
        "Building Production-Ready Next.js 16 Applications with Tailwind v4",
      speaker: "Son Nguyen",
      speakerRole: "Tech Lead - GDSC",
      date: "Saturday, Sep 05, 2026",
      time: "09:00 AM - 11:30 AM",
      location: "Room F302, DUT & Google Meet",
      track: "Frontend",
      tagColor:
        "bg-blue-50 text-blue-700 dark:bg-blue-950/50 dark:text-blue-300 border-blue-200 dark:border-blue-800",
    },
    {
      id: "2",
      title:
        "Scalable Microservices with ASP.NET Core & Event-Driven Architecture",
      speaker: "Alex Tran",
      speakerRole: "Backend Core Member",
      date: "Wednesday, Sep 09, 2026",
      time: "07:30 PM - 09:30 PM",
      location: "Google Meet",
      track: "Backend",
      tagColor:
        "bg-emerald-50 text-emerald-700 dark:bg-emerald-950/50 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800",
    },
  ];

  const displayName = user?.displayName || "Member";
  const departmentName = user?.department?.name || "Software Engineering";
  const userRoles = user?.roles || ["Member"];

  return (
    <AuthenticatedLayout user={user}>
      {/* Main Container Content */}
      <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
        {/* Welcome Hero Banner with Liquid Glass */}
        <Card
          variant="liquid-glass"
          className="border-brand-border/40 relative overflow-hidden p-6 sm:p-8"
        >
          <div className="relative z-10 flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
            <div className="max-w-2xl space-y-2">
              <div className="flex flex-wrap items-center gap-2">
                <span className="bg-brand-muted text-brand inline-flex items-center gap-1.5 rounded-full px-3 py-0.5 text-xs font-semibold">
                  <Sparkles className="size-3.5" />
                  {departmentName}
                </span>
                {userRoles.map((role) => (
                  <span
                    key={role}
                    className="inline-flex items-center rounded-full border border-neutral-200 bg-neutral-100 px-2.5 py-0.5 text-xs font-medium text-neutral-700 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-300"
                  >
                    {role}
                  </span>
                ))}
              </div>

              <h1 className="text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
                {t("dashboard.greeting")}, {displayName}!
              </h1>
              <p className="text-sm leading-relaxed text-neutral-600 dark:text-zinc-400">
                {t("dashboard.subtitle")}
              </p>
            </div>

            {/* Quick Action Button */}
            <div className="flex shrink-0 items-center gap-3">
              <Link href="/schedule">
                <Button
                  variant="brand"
                  size="md"
                  leftIcon={<PlusCircle className="size-4" />}
                  className="font-semibold shadow-md"
                >
                  {t("dashboard.createSharing")}
                </Button>
              </Link>
            </div>
          </div>
        </Card>

        {/* Search & Filter Bar */}
        <div className="w-full">
          <TextField
            placeholder={t("dashboard.searchPlaceholder")}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            startIcon={<Search className="size-4" />}
            clearable
            onClear={() => setSearchQuery("")}
            className="w-full shadow-2xs"
          />
        </div>

        {/* Grid Layout: Active Roadmaps & Upcoming Sharing Sessions */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Left 2 Columns: Active Learning Roadmaps */}
          <div className="space-y-5 lg:col-span-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <BookOpen className="text-brand size-5" />
                <h2 className="text-lg font-bold tracking-tight text-neutral-900 dark:text-zinc-100">
                  {t("dashboard.activeRoadmaps")}
                </h2>
              </div>
              <Link
                href="/roadmaps"
                className="text-brand hover:text-brand-hover flex items-center gap-1 text-xs font-semibold hover:underline"
              >
                <span>{t("dashboard.viewAllRoadmaps")}</span>
                <ChevronRight className="size-3.5" />
              </Link>
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              {roadmaps.map((rm) => (
                <Card
                  key={rm.id}
                  variant="default"
                  className={`group relative overflow-hidden bg-gradient-to-br p-5 transition-all duration-300 hover:-translate-y-1 hover:shadow-lg dark:hover:shadow-zinc-950/50 ${rm.color}`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex size-10 items-center justify-center rounded-xl bg-white shadow-2xs dark:bg-zinc-800">
                      {rm.icon}
                    </div>
                    <span className="rounded-full bg-white/80 px-2.5 py-0.5 text-[11px] font-semibold text-neutral-700 shadow-2xs dark:bg-zinc-800/80 dark:text-zinc-300">
                      {rm.badge}
                    </span>
                  </div>

                  <div className="mt-4 space-y-1">
                    <span className="text-[11px] font-medium tracking-tight text-neutral-500 dark:text-zinc-400">
                      {rm.category}
                    </span>
                    <h3 className="group-hover:text-brand text-base font-bold tracking-tight text-neutral-900 transition-colors dark:text-zinc-100">
                      {rm.title}
                    </h3>
                  </div>

                  {/* Progress bar */}
                  <div className="mt-4 space-y-1.5">
                    <div className="flex items-center justify-between text-xs text-neutral-600 dark:text-zinc-400">
                      <span>{t("dashboard.progress")}</span>
                      <span className="text-brand font-semibold">
                        {rm.progress}%
                      </span>
                    </div>
                    <div className="h-2 w-full overflow-hidden rounded-full bg-neutral-200/80 dark:bg-zinc-800">
                      <div
                        className="bg-brand h-full rounded-full transition-all duration-500"
                        style={{ width: `${rm.progress}%` }}
                      />
                    </div>
                    <div className="flex items-center justify-between pt-1 text-[11px] text-neutral-500 dark:text-zinc-500">
                      <span>
                        {rm.completedTopics}/{rm.totalTopics} topics completed
                      </span>
                    </div>
                  </div>

                  <div className="mt-4 flex justify-end border-t border-neutral-200/50 pt-3 dark:border-zinc-800/50">
                    <Link
                      href={`/roadmaps/${rm.id}`}
                      className="text-brand inline-flex items-center gap-1 text-xs font-semibold hover:underline"
                    >
                      <span>Continue</span>
                      <ArrowRight className="size-3" />
                    </Link>
                  </div>
                </Card>
              ))}
            </div>
          </div>

          {/* Right 1 Column: Upcoming Sharing Sessions */}
          <div className="space-y-5">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Calendar className="size-5 text-[#34A853]" />
                <h2 className="text-lg font-bold tracking-tight text-neutral-900 dark:text-zinc-100">
                  {t("dashboard.upcomingSessions")}
                </h2>
              </div>
              <Link
                href="/schedule"
                className="text-brand text-xs font-semibold hover:underline"
              >
                View all
              </Link>
            </div>

            <div className="space-y-4">
              {upcomingSessions.map((session) => (
                <Card
                  key={session.id}
                  variant="default"
                  className="hover:border-brand/60 dark:hover:border-brand/40 p-4 transition-all hover:shadow-md"
                >
                  <div className="flex items-start justify-between gap-2">
                    <span
                      className={`inline-block rounded-md border px-2 py-0.5 text-[10px] font-semibold ${session.tagColor}`}
                    >
                      {session.track}
                    </span>
                  </div>

                  <h3 className="mt-2 text-sm leading-snug font-bold tracking-tight text-neutral-900 dark:text-zinc-100">
                    {session.title}
                  </h3>

                  <div className="mt-3 space-y-1.5 text-xs text-neutral-600 dark:text-zinc-400">
                    <div className="flex items-center gap-2">
                      <Clock className="size-3.5 shrink-0 text-neutral-400" />
                      <span>
                        {session.date} • {session.time}
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <MapPin className="size-3.5 shrink-0 text-neutral-400" />
                      <span className="truncate">{session.location}</span>
                    </div>
                    <div className="flex items-center gap-2 pt-1">
                      <div className="bg-brand/10 text-brand flex size-5 items-center justify-center rounded-full text-[10px] font-bold">
                        {session.speaker.charAt(0)}
                      </div>
                      <span className="font-medium text-neutral-800 dark:text-zinc-200">
                        {session.speaker}
                      </span>
                      <span className="text-[11px] text-neutral-400">
                        ({session.speakerRole})
                      </span>
                    </div>
                  </div>

                  <div className="mt-4 flex justify-end border-t border-neutral-100 pt-3 dark:border-zinc-800">
                    <Button
                      variant="outline"
                      size="sm"
                      className="text-brand hover:bg-brand w-full text-xs font-semibold hover:text-white"
                    >
                      {t("dashboard.joinSession")}
                    </Button>
                  </div>
                </Card>
              ))}
            </div>
          </div>
        </div>
      </div>
    </AuthenticatedLayout>
  );
}
