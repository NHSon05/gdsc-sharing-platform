"use client";

import { useState, useEffect, useMemo } from "react";
import type { ScheduleResponse } from "../types/sharing.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface ScheduleRealtimeState {
  isLive: boolean;
  isUpcoming: boolean;
  isPast: boolean;
  badgeLabel: string;
  relativeTimeText: string | null;
}

export function useCurrentTime(intervalMs: number = 10_000): Date {
  const [now, setNow] = useState<Date>(() => new Date());

  useEffect(() => {
    const timer = setInterval(() => {
      setNow(new Date());
    }, intervalMs);
    return () => clearInterval(timer);
  }, [intervalMs]);

  return now;
}

export function getScheduleRealtimeState(
  schedule: ScheduleResponse,
  now: Date,
  locale: "en" | "vi" = "vi"
): ScheduleRealtimeState {
  // If explicitly cancelled
  if (schedule.status === "Cancelled") {
    return {
      isLive: false,
      isUpcoming: false,
      isPast: false,
      badgeLabel: locale === "vi" ? "Đã hủy" : "Cancelled",
      relativeTimeText: null,
    };
  }

  // If marked completed
  if (schedule.status === "Completed") {
    return {
      isLive: false,
      isUpcoming: false,
      isPast: true,
      badgeLabel: locale === "vi" ? "Đã hoàn thành" : "Completed",
      relativeTimeText: null,
    };
  }

  const start = new Date(schedule.startsAtUtc).getTime();
  const end = new Date(schedule.endsAtUtc).getTime();
  const current = now.getTime();

  // 1. LIVE right now (session is in progress)
  if (current >= start && current <= end) {
    return {
      isLive: true,
      isUpcoming: false,
      isPast: false,
      badgeLabel: "LIVE",
      relativeTimeText: locale === "vi" ? "Đang diễn ra" : "Happening Now",
    };
  }

  // 2. UPCOMING session (future)
  if (current < start) {
    const diffMs = start - current;
    const diffMinutes = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);

    let relative: string;
    if (diffMinutes < 1) {
      relative = locale === "vi" ? "Bắt đầu ngay" : "Starting now";
    } else if (diffMinutes < 60) {
      relative =
        locale === "vi"
          ? `Bắt đầu sau ${diffMinutes}p`
          : `Starts in ${diffMinutes}m`;
    } else if (diffHours < 24) {
      relative =
        locale === "vi"
          ? `Bắt đầu sau ${diffHours}h`
          : `Starts in ${diffHours}h`;
    } else {
      relative =
        locale === "vi" ? `Sau ${diffDays} ngày` : `In ${diffDays} days`;
    }

    return {
      isLive: false,
      isUpcoming: true,
      isPast: false,
      badgeLabel: locale === "vi" ? "Đã lên lịch" : "Scheduled",
      relativeTimeText: relative,
    };
  }

  // 3. PAST session (ended)
  return {
    isLive: false,
    isUpcoming: false,
    isPast: true,
    badgeLabel: locale === "vi" ? "Đã kết thúc" : "Ended",
    relativeTimeText: locale === "vi" ? "Đã kết thúc" : "Ended",
  };
}

export function useScheduleRealtime(
  schedule: ScheduleResponse | null | undefined,
  intervalMs: number = 10_000
): ScheduleRealtimeState | null {
  const { locale } = useTranslation();
  const now = useCurrentTime(intervalMs);

  return useMemo(() => {
    if (!schedule) return null;
    return getScheduleRealtimeState(schedule, now, locale);
  }, [schedule, now, locale]);
}
