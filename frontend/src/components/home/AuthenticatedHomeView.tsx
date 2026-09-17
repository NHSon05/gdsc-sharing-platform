"use client";

import React from "react";
import Link from "next/link";
import { format, parseISO } from "date-fns";
import { vi, enUS } from "date-fns/locale";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useTranslation } from "@/core/i18n/i18n.context";
import { AuthenticatedLayout } from "@/components/layout/AuthenticatedLayout";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import {
  BookOpen,
  Calendar,
  ArrowRight,
  Clock,
  MapPin,
  Code2,
  Cpu,
  Server,
  ChevronRight,
  ShieldCheck,
  FileText,
  Video,
} from "lucide-react";

import { useCurrentUserQuery } from "@/features/auth/hooks/use-current-user-query";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import {
  QuickShareComposer,
  InfiniteFeedStream,
  useSchedulesQuery,
  DeliveryModeBadge,
  useCurrentTime,
  getScheduleRealtimeState,
} from "@/features/sharing";

interface AuthenticatedHomeViewProps {
  user?: CurrentUserDto | null;
  accessToken?: string | null;
  refreshToken?: string | null;
}

export function AuthenticatedHomeView({
  user: initialUser,
  accessToken,
  refreshToken,
}: AuthenticatedHomeViewProps) {
  const { t, locale } = useTranslation();
  const now = useCurrentTime(10_000);
  const { data: userQuery } = useCurrentUserQuery(initialUser);
  const storeUser = useSessionStore(selectCurrentUser);
  const user = initialUser || userQuery || storeUser;

  // Fetch real upcoming sharing sessions
  const { data: schedulesData } = useSchedulesQuery({
    pageSize: 3,
  });
  const upcomingSchedules = schedulesData?.items ?? [];

  const userRoles = user?.roles || ["Member"];
  const isAdminOrLead = userRoles.some((r) =>
    ["Admin", "Lead", "SubLead", "CoreTeam"].includes(r)
  );

  const roadmaps = [
    {
      id: "frontend",
      title: "Frontend Engineering 2026",
      category: "Frontend & UI/UX",
      icon: <Code2 className="text-brand size-4" />,
      progress: 68,
      color: "border-brand-border/40 bg-brand-muted/20",
    },
    {
      id: "backend",
      title: "Clean Architecture & ASP.NET Core",
      category: "Backend & Systems",
      icon: <Server className="size-4 text-emerald-500" />,
      progress: 42,
      color:
        "border-emerald-200 dark:border-emerald-900/40 bg-emerald-50/30 dark:bg-emerald-950/20",
    },
    {
      id: "ai",
      title: "Generative AI & Agentic Workflows",
      category: "AI & Machine Learning",
      icon: <Cpu className="size-4 text-violet-500" />,
      progress: 25,
      color:
        "border-violet-200 dark:border-violet-900/40 bg-violet-50/30 dark:bg-violet-950/20",
    },
  ];

  return (
    <AuthenticatedLayout
      user={user}
      accessToken={accessToken}
      refreshToken={refreshToken}
    >
      {/* Main Container */}
      <div className="mx-auto max-w-7xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
        {/* 2-Column Community Layout */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Main Column (2/3): Creator Box & Community Feed with Infinite Scroll */}
          <div className="space-y-6 lg:col-span-2">
            {/* 1. Quick Share Composer Box */}
            <QuickShareComposer user={user} />

            {/* 2. Infinite Feed Stream */}
            <InfiniteFeedStream />
          </div>

          {/* Sidebar Column (1/3): Upcoming Sessions, Roadmaps, Quick Links */}
          <div className="space-y-6">
            {/* Upcoming Sharing Sessions Widget */}
            <Card
              variant="default"
              className="rounded-2xl border border-neutral-200/80 bg-white p-5 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900/90"
            >
              <div className="flex items-center justify-between border-b border-neutral-100 pb-3 dark:border-zinc-800">
                <div className="flex items-center gap-2">
                  <div className="bg-brand/10 text-brand flex size-7 items-center justify-center rounded-lg">
                    <Calendar className="size-4" />
                  </div>
                  <h3 className="text-sm font-bold text-neutral-900 dark:text-white">
                    {t("dashboard.upcomingSessions")}
                  </h3>
                </div>
                <Link
                  href="/schedule"
                  className="text-brand flex items-center gap-1 text-xs font-semibold hover:underline"
                >
                  <span>View all</span>
                  <ChevronRight className="size-3" />
                </Link>
              </div>

              <div className="mt-3 space-y-3">
                {upcomingSchedules.length > 0 ? (
                  upcomingSchedules.map((session) => {
                    const realtime = getScheduleRealtimeState(
                      session,
                      now,
                      locale
                    );

                    let formattedDate = "";
                    try {
                      formattedDate = format(
                        parseISO(session.startsAtUtc),
                        "EEE, dd/MM • HH:mm",
                        { locale: locale === "vi" ? vi : enUS }
                      );
                    } catch {
                      formattedDate = session.startsAtUtc;
                    }

                    return (
                      <Link
                        key={session.id}
                        href="/schedule"
                        className={`group block rounded-xl border p-3 transition-all ${
                          realtime.isLive
                            ? "border-rose-400/80 bg-rose-50/20 ring-1 ring-rose-500/20 dark:border-rose-800 dark:bg-rose-950/20"
                            : "hover:border-brand/40 hover:bg-brand-muted/10 border-neutral-100 dark:border-zinc-800/80"
                        }`}
                      >
                        <div className="flex items-center justify-between gap-2">
                          <div className="flex items-center gap-1.5">
                            <DeliveryModeBadge mode={session.deliveryMode} />
                            {realtime.isLive ? (
                              <span className="inline-flex animate-pulse items-center gap-1 rounded-full border border-rose-200 bg-rose-50 px-2 py-0.5 text-[10px] font-bold text-rose-600 dark:border-rose-900/60 dark:bg-rose-950/40 dark:text-rose-300">
                                <span className="size-1.5 animate-ping rounded-full bg-rose-500" />
                                LIVE
                              </span>
                            ) : (
                              realtime.relativeTimeText && (
                                <span className="rounded-md border border-blue-200/60 bg-blue-50/60 px-1.5 py-0.5 text-[10px] font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/40 dark:text-blue-300">
                                  {realtime.relativeTimeText}
                                </span>
                              )
                            )}
                          </div>
                          <span className="text-[10px] font-semibold tracking-wider text-neutral-400 uppercase">
                            {session.sharingType}
                          </span>
                        </div>

                        <h4 className="group-hover:text-brand mt-1.5 line-clamp-1 text-xs font-bold text-neutral-900 transition-colors dark:text-zinc-100">
                          {session.title}
                        </h4>

                        <div className="mt-2 flex items-center justify-between text-[11px] text-neutral-500 dark:text-zinc-400">
                          <span className="flex items-center gap-1">
                            <Clock className="size-3 text-neutral-400" />
                            {formattedDate}
                          </span>
                          {session.location ? (
                            <span className="flex max-w-30 items-center gap-1 truncate">
                              <MapPin className="size-3 shrink-0 text-neutral-400" />
                              <span className="truncate">
                                {session.location}
                              </span>
                            </span>
                          ) : session.meetingUrl ? (
                            <span className="flex items-center gap-1 text-sky-600 dark:text-sky-400">
                              <Video className="size-3" />
                              Online
                            </span>
                          ) : null}
                        </div>
                      </Link>
                    );
                  })
                ) : (
                  <div className="py-4 text-center">
                    <p className="text-xs text-neutral-500 dark:text-zinc-400">
                      {t("sharing.emptySchedules")}
                    </p>
                    <Link href="/schedule">
                      <Button
                        variant="brand"
                        size="sm"
                        className="mt-2 text-xs font-semibold shadow-xs"
                      >
                        {t("sharing.createSchedule")}
                      </Button>
                    </Link>
                  </div>
                )}
              </div>
            </Card>

            {/* Active Roadmaps Quick Progress Widget */}
            <Card
              variant="default"
              className="rounded-2xl border border-neutral-200/80 bg-white p-5 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900/90"
            >
              <div className="flex items-center justify-between border-b border-neutral-100 pb-3 dark:border-zinc-800">
                <div className="flex items-center gap-2">
                  <div className="flex size-7 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600 dark:bg-emerald-950/50 dark:text-emerald-400">
                    <BookOpen className="size-4" />
                  </div>
                  <h3 className="text-sm font-bold text-neutral-900 dark:text-white">
                    {t("dashboard.activeRoadmaps")}
                  </h3>
                </div>
                <Link
                  href="/roadmaps"
                  className="text-brand flex items-center gap-1 text-xs font-semibold hover:underline"
                >
                  <span>View all</span>
                  <ChevronRight className="size-3" />
                </Link>
              </div>

              <div className="mt-3 space-y-2.5">
                {roadmaps.map((rm) => (
                  <Link
                    key={rm.id}
                    href={`/roadmaps/${rm.id}`}
                    className="group hover:border-brand/40 dark:hover:border-brand/40 block rounded-xl border border-neutral-100 p-3 transition-all hover:bg-neutral-50/60 dark:border-zinc-800/80 dark:hover:bg-zinc-800/40"
                  >
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center gap-2">
                        {rm.icon}
                        <span className="group-hover:text-brand line-clamp-1 text-xs font-bold text-neutral-900 transition-colors dark:text-zinc-100">
                          {rm.title}
                        </span>
                      </div>
                      <span className="text-brand text-xs font-bold">
                        {rm.progress}%
                      </span>
                    </div>

                    <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-neutral-100 dark:bg-zinc-800">
                      <div
                        className="bg-brand h-full rounded-full transition-all duration-500"
                        style={{ width: `${rm.progress}%` }}
                      />
                    </div>
                  </Link>
                ))}
              </div>
            </Card>

            {/* Quick Shortcuts & Management Card */}
            <Card
              variant="default"
              className="from-brand-muted/30 dark:from-brand-muted/10 rounded-2xl border border-neutral-200/80 bg-linear-to-br to-sky-50/20 p-5 shadow-2xs dark:border-zinc-800 dark:to-zinc-900"
            >
              <h3 className="text-brand text-xs font-bold tracking-wider uppercase">
                Quick Shortcuts
              </h3>
              <div className="mt-3 space-y-2 text-xs">
                <Link
                  href="/sharing/mine"
                  className="hover:border-brand/40 hover:text-brand flex items-center justify-between rounded-xl border border-neutral-200/60 bg-white/80 p-2.5 font-semibold text-neutral-900 transition-all hover:shadow-2xs dark:border-zinc-700/60 dark:bg-zinc-800/80 dark:text-zinc-200"
                >
                  <span className="flex items-center gap-2">
                    <FileText className="text-brand size-4" />
                    <span>{t("sharing.myContents")}</span>
                  </span>
                  <ArrowRight className="size-4 text-neutral-400" />
                </Link>

                <Link
                  href="/schedule"
                  className="hover:border-brand/40 hover:text-brand flex items-center justify-between rounded-xl border border-neutral-200/60 bg-white/80 p-2.5 font-semibold text-neutral-900 transition-all hover:shadow-2xs dark:border-zinc-700/60 dark:bg-zinc-800/80 dark:text-zinc-200"
                >
                  <span className="flex items-center gap-2">
                    <Calendar className="size-4 text-emerald-600" />
                    <span>{t("sharing.scheduleTitle")}</span>
                  </span>
                  <ArrowRight className="size-4 text-neutral-400" />
                </Link>

                {isAdminOrLead && (
                  <Link
                    href="/admin/sharing/review"
                    className="flex items-center justify-between rounded-xl border border-amber-200/60 bg-amber-50/50 p-2.5 font-semibold text-amber-900 transition-all hover:border-amber-400 hover:shadow-2xs dark:border-amber-900/60 dark:bg-amber-950/30 dark:text-amber-200"
                  >
                    <span className="flex items-center gap-2">
                      <ShieldCheck className="size-4 text-amber-600" />
                      <span>{t("sharing.reviewQueue")}</span>
                    </span>
                    <ArrowRight className="size-4 text-amber-500" />
                  </Link>
                )}
              </div>
            </Card>
          </div>
        </div>
      </div>
    </AuthenticatedLayout>
  );
}
