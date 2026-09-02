"use client";

import * as React from "react";
import {
  format,
  addDays,
  subDays,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  isSameDay,
  isToday,
  addWeeks,
  subWeeks,
  addMonths,
  subMonths,
  addYears,
  subYears,
} from "date-fns";
import {
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  Plus,
  Check,
} from "lucide-react";
import { Calendar } from "@/components/ui/calendar";
import { useTranslation } from "@/core/i18n/i18n.context";
import { cn } from "@/lib/utils";

/* ==========================================================================
   Types & Interfaces
   ========================================================================== */

export type CalendarViewMode = "day" | "week" | "month" | "year";

export type EventColor =
  "blue" | "cyan" | "rose" | "amber" | "white" | "purple";

export interface CalendarEvent {
  id: string;
  title: string;
  startDate: Date;
  endDate: Date;
  location?: string;
  color?: EventColor;
  category?: string;
  description?: string;
  allDay?: boolean;
}

export interface CalendarCategory {
  id: string;
  label: string;
  color?: string;
  checked?: boolean;
}

export interface EventCalendarProps {
  /** Initial or controlled active date */
  date?: Date;
  /** Callback when active date changes */
  onDateChange?: (date: Date) => void;
  /** List of calendar events */
  events?: CalendarEvent[];
  /** Callback when an event card is clicked */
  onEventClick?: (event: CalendarEvent) => void;
  /** Callback when an empty time slot is clicked */
  onSlotClick?: (date: Date, hour: number) => void;
  /** Callback when the + Add event button is clicked */
  onAddEvent?: () => void;
  /** View mode: 'day' | 'week' | 'month' | 'year' */
  viewMode?: CalendarViewMode;
  /** Callback when view mode changes */
  onViewModeChange?: (mode: CalendarViewMode) => void;
  /** Filter categories in sidebar */
  categories?: CalendarCategory[];
  /** Callback when category filter changes */
  onCategoryToggle?: (categoryId: string, checked: boolean) => void;
  /** User account label displayed in sidebar */
  accountEmail?: string;
  /** Timezone label (e.g. "GMT +7") */
  timezone?: string;
  /** Start hour for schedule grid (default: 0 for 00:00 or 1 for 01:00) */
  startHour?: number;
  /** End hour for schedule grid (default: 23 for 23:00) */
  endHour?: number;
  /** Custom container class */
  className?: string;
}

/* ==========================================================================
   Color Theme Mapping
   ========================================================================== */

const EVENT_COLOR_STYLES: Record<EventColor, string> = {
  blue: "bg-[#EBF3FE] border-[#C2DBFE] text-[#1E40AF] dark:bg-blue-950/50 dark:border-blue-800/70 dark:text-blue-200",
  cyan: "bg-[#E6F9F6] border-[#B5EFE7] text-[#0D9488] dark:bg-teal-950/50 dark:border-teal-800/70 dark:text-teal-200",
  rose: "bg-[#FEECEC] border-[#FDC5C5] text-[#BE123C] dark:bg-rose-950/50 dark:border-rose-800/70 dark:text-rose-200",
  amber:
    "bg-[#FEF6E6] border-[#FDE3B2] text-[#B45309] dark:bg-amber-950/50 dark:border-amber-800/70 dark:text-amber-200",
  purple:
    "bg-[#F3E8FF] border-[#E9D5FF] text-[#7E22CE] dark:bg-purple-950/50 dark:border-purple-800/70 dark:text-purple-200",
  white:
    "bg-white border-neutral-200/90 text-neutral-900 shadow-2xs dark:bg-zinc-900/90 dark:border-zinc-800 dark:text-zinc-100",
};

/* ==========================================================================
   EventCalendar Component
   ========================================================================== */

export function EventCalendar({
  date: controlledDate,
  onDateChange,
  events = [],
  onEventClick,
  onSlotClick,
  onAddEvent,
  viewMode: controlledViewMode,
  onViewModeChange,
  categories,
  onCategoryToggle,
  accountEmail = "Hoangthuan0112@gmail.com",
  timezone = "GMT +7",
  startHour = 0,
  endHour = 12,
  className,
}: EventCalendarProps) {
  const { t } = useTranslation();

  const defaultCategories = React.useMemo<CalendarCategory[]>(
    () => [
      { id: "1", label: t("schedule.clubEvents"), checked: true },
      { id: "2", label: t("schedule.techTalks"), checked: false },
      { id: "3", label: t("schedule.internalMeetings"), checked: false },
      { id: "4", label: t("schedule.projectMilestones"), checked: false },
    ],
    [t]
  );

  const [uncontrolledDate, setUncontrolledDate] = React.useState<Date>(
    new Date(2025, 11, 9)
  );
  const activeDate =
    controlledDate !== undefined ? controlledDate : uncontrolledDate;

  const [uncontrolledViewMode, setUncontrolledViewMode] =
    React.useState<CalendarViewMode>("week");
  const activeViewMode =
    controlledViewMode !== undefined
      ? controlledViewMode
      : uncontrolledViewMode;
  const [checkedMap, setCheckedMap] = React.useState<Record<string, boolean>>(
    {}
  );

  const categoryList = React.useMemo(() => {
    const source = categories || defaultCategories;
    return source.map((c) => ({
      ...c,
      checked: checkedMap[c.id] !== undefined ? checkedMap[c.id] : c.checked,
    }));
  }, [categories, defaultCategories, checkedMap]);

  const handleDateChange = (newDate: Date) => {
    if (controlledDate === undefined) {
      setUncontrolledDate(newDate);
    }
    onDateChange?.(newDate);
  };

  const handleViewModeChange = (mode: CalendarViewMode) => {
    if (controlledViewMode === undefined) {
      setUncontrolledViewMode(mode);
    }
    onViewModeChange?.(mode);
  };

  const handlePrev = () => {
    if (activeViewMode === "week") handleDateChange(subWeeks(activeDate, 1));
    else if (activeViewMode === "month")
      handleDateChange(subMonths(activeDate, 1));
    else if (activeViewMode === "day") handleDateChange(subDays(activeDate, 1));
    else if (activeViewMode === "year")
      handleDateChange(subYears(activeDate, 1));
  };

  const handleNext = () => {
    if (activeViewMode === "week") handleDateChange(addWeeks(activeDate, 1));
    else if (activeViewMode === "month")
      handleDateChange(addMonths(activeDate, 1));
    else if (activeViewMode === "day") handleDateChange(addDays(activeDate, 1));
    else if (activeViewMode === "year")
      handleDateChange(addYears(activeDate, 1));
  };

  const handleToday = () => {
    handleDateChange(new Date());
  };

  const handleToggleCategory = (id: string) => {
    const currentItem = categoryList.find((c) => c.id === id);
    const nextChecked = !currentItem?.checked;
    setCheckedMap((prev) => ({ ...prev, [id]: nextChecked }));
    onCategoryToggle?.(id, nextChecked);
  };

  // Week days array (Monday-first)
  const weekStartDate = startOfWeek(activeDate, { weekStartsOn: 1 });
  const weekEndDate = endOfWeek(activeDate, { weekStartsOn: 1 });
  const weekDays = eachDayOfInterval({
    start: weekStartDate,
    end: weekEndDate,
  });

  // Hours array
  const hours = Array.from(
    { length: endHour - startHour + 1 },
    (_, i) => startHour + i
  );

  const viewModes: { key: CalendarViewMode; label: string }[] = [
    { key: "day", label: t("schedule.day") },
    { key: "week", label: t("schedule.week") },
    { key: "month", label: t("schedule.month") },
    { key: "year", label: t("schedule.year") },
  ];

  return (
    <div
      className={cn(
        "flex w-full flex-col gap-6 border border-neutral-200/80 bg-white p-6 font-sans shadow-sm transition-colors select-none lg:flex-row dark:border-zinc-800/80 dark:bg-[#0C0C0E]",
        className
      )}
    >
      {/* ====================================================================
          LEFT SIDEBAR: Add Event + Mini Calendar + Category Checkboxes
          ==================================================================== */}
      <div className="w-full shrink-0 space-y-6 lg:w-72">
        {/* + Add event Button with Brand Color */}
        <button
          type="button"
          onClick={onAddEvent}
          className="bg-brand text-brand-foreground hover:bg-brand-hover flex w-full cursor-pointer items-center justify-center gap-2 rounded-2xl px-4 py-3 text-sm font-semibold shadow-md transition-all hover:shadow-lg active:scale-[0.98]"
        >
          <Plus className="size-4 stroke-[2.5]" />
          <span>{t("schedule.addEvent")}</span>
        </button>

        {/* Mini Single Date Calendar */}
        <div className="flex justify-center">
          <Calendar
            value={activeDate}
            onChange={handleDateChange}
            showYearPicker={true}
            className="w-full border-0 p-0 shadow-none dark:bg-transparent"
          />
        </div>

        {/* Account Email Dropdown & Category Checkboxes */}
        <div className="space-y-3 border-t border-neutral-200/70 pt-5 dark:border-zinc-800/70">
          <button
            type="button"
            className="flex w-full items-center justify-between text-xs font-semibold text-neutral-800 hover:text-neutral-950 dark:text-zinc-200 dark:hover:text-white"
          >
            <span className="truncate">{accountEmail}</span>
            <ChevronDown className="size-3.5 shrink-0 text-neutral-400" />
          </button>

          <div className="space-y-2.5 pt-1">
            {categoryList.map((cat) => (
              <label
                key={cat.id}
                onClick={() => handleToggleCategory(cat.id)}
                className="flex cursor-pointer items-center gap-2.5 text-xs font-medium text-neutral-700 hover:text-neutral-950 dark:text-zinc-300 dark:hover:text-white"
              >
                <div
                  className={cn(
                    "flex size-4 items-center justify-center rounded-md border transition-all duration-150",
                    cat.checked
                      ? "border-brand bg-brand text-white shadow-2xs"
                      : "border-neutral-300 bg-white hover:border-neutral-400 dark:border-zinc-700 dark:bg-zinc-900"
                  )}
                >
                  {cat.checked && <Check className="size-3 stroke-[3]" />}
                </div>
                <span>{cat.label}</span>
              </label>
            ))}
          </div>
        </div>
      </div>

      {/* ====================================================================
          RIGHT MAIN AREA: Top Control Bar + Week Schedule Grid
          ==================================================================== */}
      <div className="flex min-w-0 flex-1 flex-col">
        {/* Top Control Header Bar */}
        <div className="flex flex-col gap-4 border-b border-neutral-200/80 pb-5 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800/80">
          {/* Left Month & Nav Buttons */}
          <div className="flex items-center gap-4">
            <h2 className="text-xl font-bold tracking-tight text-neutral-900 sm:text-2xl dark:text-white">
              {format(activeDate, "MMMM yyyy")}
            </h2>

            <div className="flex items-center gap-1.5">
              <button
                type="button"
                onClick={handlePrev}
                aria-label="Previous"
                className="flex size-8 cursor-pointer items-center justify-center rounded-xl border border-neutral-200/80 bg-white text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-950 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
              >
                <ChevronLeft className="size-4" />
              </button>

              <button
                type="button"
                onClick={handleToday}
                className="cursor-pointer rounded-xl border border-neutral-200/80 bg-white px-3 py-1.5 text-xs font-semibold text-neutral-800 transition-colors hover:bg-neutral-100 hover:text-neutral-950 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-200 dark:hover:bg-zinc-800 dark:hover:text-white"
              >
                {t("schedule.today")}
              </button>

              <button
                type="button"
                onClick={handleNext}
                aria-label="Next"
                className="flex size-8 cursor-pointer items-center justify-center rounded-xl border border-neutral-200/80 bg-white text-neutral-600 transition-colors hover:bg-neutral-100 hover:text-neutral-950 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800 dark:hover:text-white"
              >
                <ChevronRight className="size-4" />
              </button>
            </div>
          </div>

          {/* Right View Mode Pill Group: Day | Week | Month | Year */}
          <div className="flex items-center rounded-2xl border border-neutral-200/80 bg-neutral-100/80 p-1 dark:border-zinc-800 dark:bg-zinc-900">
            {viewModes.map((vm) => (
              <button
                key={vm.key}
                type="button"
                onClick={() => handleViewModeChange(vm.key)}
                className={cn(
                  "cursor-pointer rounded-xl px-3.5 py-1.5 text-xs font-semibold transition-all duration-150",
                  activeViewMode === vm.key
                    ? "text-brand dark:text-brand-hover bg-white font-bold shadow-xs dark:bg-zinc-800"
                    : "text-neutral-500 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white"
                )}
              >
                {vm.label}
              </button>
            ))}
          </div>
        </div>

        {/* Schedule Grid Table */}
        <div className="flex-1 overflow-x-auto">
          <div className="w-full min-w-[750px]">
            {/* Week Header Row */}
            <div className="grid grid-cols-[64px_repeat(7,minmax(0,1fr))] border-b border-neutral-200/80 text-center dark:border-zinc-800/80">
              {/* Timezone header */}
              <div className="flex items-center justify-center border-r border-neutral-200/60 py-3 text-[11px] font-semibold text-neutral-400 dark:border-zinc-800/60 dark:text-zinc-500">
                {timezone}
              </div>

              {/* 7 Days columns */}
              {weekDays.map((day) => {
                const dayIsToday = isToday(day);
                const dayIsActive = isSameDay(day, activeDate);

                return (
                  <div
                    key={day.toISOString()}
                    onClick={() => handleDateChange(day)}
                    className="flex min-w-0 cursor-pointer flex-col items-center justify-center border-r border-neutral-200/60 py-2.5 transition-colors last:border-r-0 hover:bg-neutral-50/50 dark:border-zinc-800/60 dark:hover:bg-zinc-900/30"
                  >
                    <div
                      className={cn(
                        "flex flex-col items-center justify-center rounded-2xl px-3 py-1 transition-all",
                        dayIsActive || dayIsToday
                          ? "border-brand bg-brand/10 text-brand dark:border-brand dark:bg-brand/15 dark:text-brand-hover border-2 font-bold shadow-xs"
                          : "text-neutral-600 dark:text-zinc-400"
                      )}
                    >
                      <span className="text-sm leading-tight font-bold">
                        {format(day, "d")}
                      </span>
                      <span className="text-[11px] font-medium tracking-tight">
                        {format(day, "EEE")}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* All Day Row */}
            <div className="grid min-h-8 grid-cols-[64px_repeat(7,minmax(0,1fr))] border-b border-neutral-200/80 dark:border-zinc-800/80">
              <div className="flex items-center justify-center border-r border-neutral-200/60 text-[11px] font-medium text-neutral-400 dark:border-zinc-800/60 dark:text-zinc-500">
                {t("schedule.allDay")}
              </div>
              {weekDays.map((day) => (
                <div
                  key={day.toISOString()}
                  className="min-w-0 border-r border-neutral-200/60 last:border-r-0 dark:border-zinc-800/60"
                />
              ))}
            </div>

            {/* Hourly Time Slots Body with Events */}
            <div className="relative">
              {/* Current Time Indicator Line with Brand Theme */}
              <div
                className="pointer-events-none absolute right-0 left-0 z-20 flex items-center"
                style={{ top: "480px" }}
              >
                <div className="flex w-[64px] justify-center">
                  <span className="bg-brand rounded-full px-1.5 py-0.5 text-[10px] font-bold text-white shadow-xs">
                    08:30
                  </span>
                </div>
                <div className="border-brand relative flex-1 border-t-2 border-dashed">
                  <span className="bg-brand absolute -top-1 left-0 size-2 rounded-full ring-2 ring-white dark:ring-zinc-950" />
                </div>
              </div>

              {/* Grid Rows for each hour */}
              {hours.map((hour) => (
                <div
                  key={hour}
                  className="grid h-18 grid-cols-[64px_repeat(7,minmax(0,1fr))] border-b border-neutral-100 dark:border-zinc-900"
                >
                  {/* Hour Label */}
                  <div className="flex items-start justify-center border-r border-neutral-200/60 pt-2 text-[11px] font-medium text-neutral-400 dark:border-zinc-800/60 dark:text-zinc-500">
                    {String(hour).padStart(2, "0")}:00
                  </div>

                  {/* 7 Day Slot Cells */}
                  {weekDays.map((day) => {
                    // Filter events occurring in this day & hour
                    const dayEvents = events.filter((ev) => {
                      const isSameDate = isSameDay(ev.startDate, day);
                      const startH = ev.startDate.getHours();
                      return isSameDate && startH === hour;
                    });

                    return (
                      <div
                        key={day.toISOString()}
                        onClick={() => onSlotClick?.(day, hour)}
                        className="relative min-w-0 overflow-hidden border-r border-neutral-200/60 p-1 transition-colors last:border-r-0 hover:bg-neutral-50/40 dark:border-zinc-800/60 dark:hover:bg-zinc-900/20"
                      >
                        {dayEvents.map((ev) => {
                          const colorStyle =
                            EVENT_COLOR_STYLES[ev.color || "blue"];

                          return (
                            <div
                              key={ev.id}
                              onClick={(e) => {
                                e.stopPropagation();
                                onEventClick?.(ev);
                              }}
                              className={cn(
                                "group z-10 block w-full min-w-0 cursor-pointer overflow-hidden rounded-xl border p-2 text-left transition-all duration-150 hover:scale-[1.01] hover:shadow-md",
                                colorStyle
                              )}
                            >
                              <h4 className="truncate text-xs leading-snug font-bold">
                                {ev.title}
                              </h4>
                              <div className="mt-1 flex flex-col gap-0.5 text-[10px] leading-none opacity-80">
                                <span className="truncate">
                                  {format(ev.startDate, "HH:mm")}
                                  {ev.endDate &&
                                    ` - ${format(ev.endDate, "HH:mm")}`}
                                </span>
                                {ev.location && (
                                  <span className="mt-0.5 truncate">
                                    {ev.location}
                                  </span>
                                )}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    );
                  })}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
