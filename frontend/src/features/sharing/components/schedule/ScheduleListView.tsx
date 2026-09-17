"use client";

import React, { useState, useMemo } from "react";
import { format, isSameDay, isToday, isTomorrow } from "date-fns";
import type { ScheduleResponse } from "../../types/sharing.types";
import { ScheduleCard } from "./ScheduleCard";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  Calendar as CalendarIcon,
  CalendarDays,
  Loader2,
  Plus,
  Search,
  X,
  RotateCcw,
} from "lucide-react";
import { Calendar } from "@/components/ui/calendar";
import { cn } from "@/lib/utils";
import { useCurrentTime } from "../../hooks/use-schedule-realtime";

interface ScheduleListViewProps {
  schedules: ScheduleResponse[];
  isLoading?: boolean;
  onSelectSchedule: (schedule: ScheduleResponse) => void;
  onAddSchedule?: () => void;
  sidebarFilters?: React.ReactNode;
  headerActions?: React.ReactNode;
  className?: string;
}

export function ScheduleListView({
  schedules,
  isLoading,
  onSelectSchedule,
  onAddSchedule,
  sidebarFilters,
  headerActions,
  className,
}: ScheduleListViewProps) {
  const { t } = useTranslation();
  const now = useCurrentTime(10_000);

  // Filter states
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [quickFilter, setQuickFilter] = useState<
    "all" | "upcoming" | "live" | "past"
  >("all");

  // Filtered schedules based on search, date, and quickFilter
  const displayedSchedules = useMemo(() => {
    const currentTime = now.getTime();

    return schedules.filter((item) => {
      // 1. Date filter (from mini calendar)
      if (selectedDate) {
        const itemDate = new Date(item.startsAtUtc);
        if (!isSameDay(itemDate, selectedDate)) {
          return false;
        }
      }

      // 2. Search query filter
      if (searchQuery.trim()) {
        const q = searchQuery.toLowerCase().trim();
        const matchesTitle = item.title.toLowerCase().includes(q);
        const matchesDesc = item.description?.toLowerCase().includes(q);
        const matchesLoc = item.location?.toLowerCase().includes(q);
        const matchesPresenter = item.presenters.some((p) =>
          p.fullName.toLowerCase().includes(q)
        );

        if (!matchesTitle && !matchesDesc && !matchesLoc && !matchesPresenter) {
          return false;
        }
      }

      // 3. Quick filter
      const start = new Date(item.startsAtUtc).getTime();
      const end = new Date(item.endsAtUtc).getTime();

      if (quickFilter === "upcoming") {
        return (
          end >= currentTime &&
          item.status !== "Cancelled" &&
          item.status !== "Completed"
        );
      }
      if (quickFilter === "live") {
        return (
          currentTime >= start &&
          currentTime <= end &&
          item.status !== "Cancelled" &&
          item.status !== "Completed"
        );
      }
      if (quickFilter === "past") {
        return (
          end < currentTime ||
          item.status === "Completed" ||
          item.status === "Cancelled"
        );
      }

      return true;
    });
  }, [schedules, selectedDate, searchQuery, quickFilter, now]);

  // Group displayed schedules by date
  const groupedByDate = useMemo(() => {
    const groups: {
      date: Date;
      dateLabel: string;
      isToday: boolean;
      isTomorrow: boolean;
      items: ScheduleResponse[];
    }[] = [];

    displayedSchedules.forEach((item) => {
      const itemDate = new Date(item.startsAtUtc);
      const dateLabel = format(itemDate, "EEEE, MMMM d, yyyy");

      let group = groups.find((g) => g.dateLabel === dateLabel);
      if (!group) {
        group = {
          date: itemDate,
          dateLabel,
          isToday: isToday(itemDate),
          isTomorrow: isTomorrow(itemDate),
          items: [],
        };
        groups.push(group);
      }
      group.items.push(item);
    });

    // Sort groups chronologically
    groups.sort((a, b) => a.date.getTime() - b.date.getTime());

    // Sort items inside each group by start time
    groups.forEach((g) => {
      g.items.sort(
        (a, b) =>
          new Date(a.startsAtUtc).getTime() - new Date(b.startsAtUtc).getTime()
      );
    });

    return groups;
  }, [displayedSchedules]);

  const handleResetSearchAndDate = () => {
    setSelectedDate(null);
    setSearchQuery("");
    setQuickFilter("all");
  };

  return (
    <div
      className={cn(
        "flex w-full flex-col gap-6 rounded-2xl border border-neutral-200/80 bg-white p-6 font-sans shadow-sm transition-colors lg:flex-row dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
        className
      )}
    >
      {/* ====================================================================
          LEFT SIDEBAR: Add Event + Mini Calendar + Filter Checkboxes
          ==================================================================== */}
      <div className="w-full shrink-0 space-y-6 lg:w-72">
        {/* + Add event Button with Brand Color */}
        {onAddSchedule && (
          <button
            type="button"
            onClick={onAddSchedule}
            className="bg-brand text-brand-foreground hover:bg-brand-hover flex w-full cursor-pointer items-center justify-center gap-2 rounded-2xl px-4 py-3 text-sm font-semibold shadow-md transition-all hover:shadow-lg active:scale-[0.98]"
          >
            <Plus className="size-4 stroke-[2.5]" />
            <span>{t("schedule.addEvent")}</span>
          </button>
        )}

        {/* Mini Calendar for Date Filtering */}
        <div className="flex justify-center">
          <Calendar
            value={selectedDate ?? undefined}
            onChange={(date) => {
              if (selectedDate && isSameDay(date, selectedDate)) {
                setSelectedDate(null);
              } else {
                setSelectedDate(date);
              }
            }}
            showYearPicker={true}
            className="w-full border-0 p-0 shadow-none dark:bg-transparent"
          />
        </div>

        {/* Sidebar Filters: Delivery Mode & Session Type Checkboxes */}
        {sidebarFilters}
      </div>

      {/* ====================================================================
          RIGHT MAIN AREA: Header + Search/Filters + Horizontal List Cards
          ==================================================================== */}
      <div className="flex min-w-0 flex-1 flex-col space-y-6">
        {/* Top Control Header Bar */}
        <div className="flex flex-col gap-4 border-b border-neutral-200/80 pb-5 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800/80">
          {/* Left Title & Status */}
          <div className="flex flex-wrap items-center gap-3">
            <h2 className="text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl dark:text-white">
              {t("sharing.listView")}
            </h2>
            <span className="rounded-full bg-neutral-100 px-2.5 py-2 text-xs font-semibold text-neutral-600 dark:bg-zinc-800 dark:text-zinc-300">
              {displayedSchedules.length}{" "}
              {displayedSchedules.length === 1 ? "session" : "sessions"}
            </span>

            {selectedDate && (
              <div className="border-brand/30 bg-brand/10 text-brand dark:border-brand/40 dark:bg-brand/20 flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-semibold">
                <CalendarIcon className="size-3" />
                <span>{format(selectedDate, "dd/MM/yyyy")}</span>
                <button
                  type="button"
                  onClick={() => setSelectedDate(null)}
                  className="hover:text-brand-hover ml-0.5 cursor-pointer"
                  aria-label="Clear date filter"
                >
                  <X className="size-3" />
                </button>
              </div>
            )}
          </div>

          {/* Right Controls: Header Actions (View Mode Toggle) */}
          {headerActions}
        </div>

        {/* Search & Quick Status Filter Bar */}
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          {/* Search Input */}
          <div className="relative max-w-md flex-1">
            <Search className="absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-neutral-400" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm kiếm theo tiêu đề, diễn giả, địa điểm..."
              className="focus:border-brand focus:ring-brand w-full rounded-xl border border-neutral-200/80 bg-neutral-50/50 py-2 pr-8 pl-9.5 text-xs placeholder:text-neutral-400 focus:bg-white focus:ring-1 focus:outline-none dark:border-zinc-800 dark:bg-zinc-900/50 dark:placeholder:text-zinc-500 dark:focus:bg-zinc-900"
            />
            {searchQuery && (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                className="absolute top-1/2 right-2.5 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 dark:hover:text-zinc-200"
              >
                <X className="size-3.5" />
              </button>
            )}
          </div>

          {/* Quick Filter Tabs: All | Upcoming | Live | Past */}
          <div className="flex items-center rounded-xl border border-neutral-200/80 bg-neutral-100/70 p-0.5 text-xs font-semibold dark:border-zinc-800 dark:bg-zinc-900">
            <button
              type="button"
              onClick={() => setQuickFilter("all")}
              className={`cursor-pointer rounded-lg px-2.5 py-1 transition-all ${
                quickFilter === "all"
                  ? "bg-white text-neutral-900 shadow-xs dark:bg-zinc-800 dark:text-white"
                  : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
              }`}
            >
              Tất cả
            </button>
            <button
              type="button"
              onClick={() => setQuickFilter("upcoming")}
              className={`cursor-pointer rounded-lg px-2.5 py-1 transition-all ${
                quickFilter === "upcoming"
                  ? "bg-white text-neutral-900 shadow-xs dark:bg-zinc-800 dark:text-white"
                  : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
              }`}
            >
              Sắp diễn ra
            </button>
            <button
              type="button"
              onClick={() => setQuickFilter("live")}
              className={`flex cursor-pointer items-center gap-1 rounded-lg px-2.5 py-1 transition-all ${
                quickFilter === "live"
                  ? "bg-white text-rose-600 shadow-xs dark:bg-zinc-800 dark:text-rose-400"
                  : "text-neutral-500 hover:text-rose-600 dark:text-zinc-400 dark:hover:text-rose-400"
              }`}
            >
              <span className="size-1.5 rounded-full bg-rose-500" />
              <span>LIVE</span>
            </button>
            <button
              type="button"
              onClick={() => setQuickFilter("past")}
              className={`cursor-pointer rounded-lg px-2.5 py-1 transition-all ${
                quickFilter === "past"
                  ? "bg-white text-neutral-900 shadow-xs dark:bg-zinc-800 dark:text-white"
                  : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
              }`}
            >
              Đã kết thúc
            </button>
          </div>
        </div>

        {/* Content Body: Loading / Empty / Grouped Sessions */}
        {isLoading ? (
          <div className="flex min-h-60 items-center justify-center">
            <Loader2 className="size-8 animate-spin text-neutral-400" />
          </div>
        ) : displayedSchedules.length === 0 ? (
          <div className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-neutral-200 bg-neutral-50/50 p-8 text-center dark:border-zinc-800 dark:bg-zinc-900/30">
            <div className="flex size-14 items-center justify-center rounded-2xl bg-neutral-100 text-neutral-400 dark:bg-zinc-800 dark:text-zinc-500">
              <CalendarDays className="size-7" />
            </div>
            <h3 className="mt-4 text-base font-bold text-neutral-900 dark:text-white">
              {t("sharing.emptySchedules")}
            </h3>
            <p className="mt-1 max-w-sm text-xs text-neutral-500 dark:text-zinc-400">
              {searchQuery || selectedDate || quickFilter !== "all"
                ? "Không tìm thấy buổi chia sẻ nào phù hợp với bộ lọc hiện tại."
                : t("sharing.emptySchedulesDesc")}
            </p>
            {(searchQuery || selectedDate || quickFilter !== "all") && (
              <button
                type="button"
                onClick={handleResetSearchAndDate}
                className="mt-4 inline-flex cursor-pointer items-center gap-1.5 rounded-xl border border-neutral-200 bg-white px-3 py-1.5 text-xs font-semibold text-neutral-700 shadow-2xs hover:bg-neutral-50 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-200"
              >
                <RotateCcw className="size-3.5" />
                <span>Đặt lại tìm kiếm</span>
              </button>
            )}
          </div>
        ) : (
          <div className="space-y-8">
            {groupedByDate.map((group) => (
              <div key={group.dateLabel} className="space-y-3">
                {/* Section Date Header */}
                <div className="flex items-center gap-3 pt-2">
                  <div className="flex items-center gap-2">
                    <CalendarDays className="size-4 text-neutral-400 dark:text-zinc-500" />
                    <h3 className="text-sm font-bold tracking-tight text-neutral-900 dark:text-zinc-200">
                      {group.dateLabel}
                    </h3>
                    {group.isToday && (
                      <span className="bg-brand/10 text-brand rounded-full px-2 py-0.5 text-[10px] font-bold">
                        Hôm nay
                      </span>
                    )}
                    {group.isTomorrow && (
                      <span className="rounded-full bg-sky-100 px-2 py-0.5 text-[10px] font-bold text-sky-700 dark:bg-sky-950/60 dark:text-sky-300">
                        Ngày mai
                      </span>
                    )}
                  </div>
                  <div className="h-px flex-1 bg-neutral-200/70 dark:bg-zinc-800/70" />
                  <span className="text-xs font-semibold text-neutral-400 dark:text-zinc-500">
                    {group.items.length}{" "}
                    {group.items.length === 1 ? "buổi chia sẻ" : "buổi chia sẻ"}
                  </span>
                </div>

                {/* Horizontal List Cards Stack */}
                <div className="space-y-3">
                  {group.items.map((schedule) => (
                    <ScheduleCard
                      key={schedule.id}
                      schedule={schedule}
                      layout="horizontal"
                      onClick={onSelectSchedule}
                    />
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
