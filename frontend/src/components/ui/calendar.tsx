"use client";

import * as React from "react";
import {
  addMonths,
  subMonths,
  startOfMonth,
  endOfMonth,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  isSameMonth,
  isSameDay,
  isToday,
  isWithinInterval,
  format,
  subDays,
  subWeeks,
  subYears,
  setMonth,
  setYear,
  getYear,
  getMonth,
  isAfter,
  isBefore,
} from "date-fns";
import {
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  ChevronUp,
} from "lucide-react";
import { cn } from "@/lib/utils";

/* ==========================================================================
   Types & Interfaces
   ========================================================================== */

export type DateRange = {
  from?: Date;
  to?: Date;
};

export interface CalendarProps {
  /** Selected date for single picker mode */
  value?: Date;
  /** Default date */
  defaultValue?: Date;
  /** Callback when date changes */
  onChange?: (date: Date) => void;
  /** Selected range for range picker mode */
  rangeValue?: DateRange;
  /** Callback when range changes */
  onRangeChange?: (range: DateRange) => void;
  /** Mode of calendar */
  mode?: "single" | "range";
  /** Minimum selectable date */
  minDate?: Date;
  /** Maximum selectable date */
  maxDate?: Date;
  /** Custom class name */
  className?: string;
  /** Show month/year picker toggle */
  showYearPicker?: boolean;
}

export interface DateRangePickerProps {
  /** Selected range */
  value?: DateRange;
  /** Default range */
  defaultValue?: DateRange;
  /** Callback when range changes */
  onChange?: (range: DateRange) => void;
  /** Custom class name */
  className?: string;
  /** Show presets sidebar */
  showPresets?: boolean;
  /** Custom presets */
  presets?: { label: string; getValue: () => DateRange }[];
}

const WEEKDAYS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

const MONTH_NAMES = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

/* ==========================================================================
   Single Date Picker & Calendar with Year Picker View
   ========================================================================== */

export function Calendar({
  value,
  defaultValue,
  onChange,
  rangeValue,
  onRangeChange,
  mode = "single",
  minDate,
  maxDate,
  className,
  showYearPicker = true,
}: CalendarProps) {
  const [uncontrolledDate, setUncontrolledDate] = React.useState<
    Date | undefined
  >(defaultValue);
  const selectedDate = value !== undefined ? value : uncontrolledDate;

  const [currentDate, setCurrentDate] = React.useState<Date>(() => {
    return value || defaultValue || rangeValue?.from || new Date();
  });

  const [isYearPickerOpen, setIsYearPickerOpen] = React.useState(false);

  const handlePrevMonth = () => {
    setCurrentDate((prev) => subMonths(prev, 1));
  };

  const handleNextMonth = () => {
    setCurrentDate((prev) => addMonths(prev, 1));
  };

  const handleSelectDay = (day: Date) => {
    if (mode === "single") {
      if (value === undefined) {
        setUncontrolledDate(day);
      }
      onChange?.(day);
    } else if (mode === "range") {
      if (!rangeValue?.from || (rangeValue.from && rangeValue.to)) {
        onRangeChange?.({ from: day, to: undefined });
      } else if (rangeValue.from && !rangeValue.to) {
        if (isBefore(day, rangeValue.from)) {
          onRangeChange?.({ from: day, to: rangeValue.from });
        } else {
          onRangeChange?.({ from: rangeValue.from, to: day });
        }
      }
    }
  };

  // Generate days for current month (Monday-first)
  const monthStart = startOfMonth(currentDate);
  const monthEnd = endOfMonth(currentDate);
  const startDate = startOfWeek(monthStart, { weekStartsOn: 1 });
  const endDate = endOfWeek(monthEnd, { weekStartsOn: 1 });
  const days = eachDayOfInterval({ start: startDate, end: endDate });

  return (
    <div
      className={cn(
        "w-full max-w-[320px] rounded-3xl border border-neutral-200/90 bg-white p-5 shadow-sm transition-colors select-none dark:border-zinc-800/90 dark:bg-zinc-950",
        className
      )}
    >
      {/* Calendar Header */}
      <div className="flex items-center justify-between">
        <button
          type="button"
          onClick={handlePrevMonth}
          disabled={isYearPickerOpen}
          className="flex size-8 cursor-pointer items-center justify-center rounded-full text-neutral-600 transition-colors hover:bg-neutral-100 disabled:opacity-30 dark:text-zinc-400 dark:hover:bg-zinc-800"
          aria-label="Previous month"
        >
          <ChevronLeft className="size-4" />
        </button>

        <button
          type="button"
          onClick={() =>
            showYearPicker && setIsYearPickerOpen(!isYearPickerOpen)
          }
          className={cn(
            "flex items-center gap-1.5 rounded-xl px-2.5 py-1 text-sm font-bold text-neutral-900 transition-colors hover:bg-neutral-100 dark:text-zinc-100 dark:hover:bg-zinc-800",
            !showYearPicker && "cursor-default hover:bg-transparent"
          )}
        >
          <span>{format(currentDate, "MMMM yyyy")}</span>
          {showYearPicker &&
            (isYearPickerOpen ? (
              <ChevronUp className="size-3.5 text-neutral-500" />
            ) : (
              <ChevronDown className="size-3.5 text-neutral-500" />
            ))}
        </button>

        <button
          type="button"
          onClick={handleNextMonth}
          disabled={isYearPickerOpen}
          className="flex size-8 cursor-pointer items-center justify-center rounded-full text-neutral-600 transition-colors hover:bg-neutral-100 disabled:opacity-30 dark:text-zinc-400 dark:hover:bg-zinc-800"
          aria-label="Next month"
        >
          <ChevronRight className="size-4" />
        </button>
      </div>

      {/* Body: Day Grid OR Year/Month Picker List */}
      {isYearPickerOpen ? (
        <YearMonthPicker
          currentDate={currentDate}
          onSelect={(newDate) => {
            setCurrentDate(newDate);
            setIsYearPickerOpen(false);
          }}
        />
      ) : (
        <div className="mt-4">
          {/* Weekday headers (Mon - Sun) */}
          <div className="grid grid-cols-7 text-center text-xs font-medium text-neutral-500 dark:text-zinc-400">
            {WEEKDAYS.map((day) => (
              <div key={day} className="py-1">
                {day}
              </div>
            ))}
          </div>

          {/* Day Grid */}
          <div className="mt-2 grid grid-cols-7 gap-y-1">
            {days.map((day) => {
              const isCurrentMonth = isSameMonth(day, currentDate);
              const isDayToday = isToday(day);
              const isSingleSelected =
                mode === "single" &&
                selectedDate &&
                isSameDay(day, selectedDate);

              const isRangeStart =
                mode === "range" &&
                rangeValue?.from &&
                isSameDay(day, rangeValue.from);
              const isRangeEnd =
                mode === "range" &&
                rangeValue?.to &&
                isSameDay(day, rangeValue.to);
              const isRangeMiddle =
                mode === "range" &&
                rangeValue?.from &&
                rangeValue?.to &&
                isWithinInterval(day, {
                  start: rangeValue.from,
                  end: rangeValue.to,
                }) &&
                !isRangeStart &&
                !isRangeEnd;

              const isDisabled =
                (minDate && isBefore(day, minDate)) ||
                (maxDate && isAfter(day, maxDate));

              if (!isCurrentMonth) {
                return (
                  <div
                    key={day.toISOString()}
                    className="flex size-9 items-center justify-center"
                  />
                );
              }

              return (
                <div
                  key={day.toISOString()}
                  className={cn(
                    "relative flex size-9 items-center justify-center p-0 text-xs font-normal",
                    isRangeMiddle && "bg-neutral-100 dark:bg-zinc-800",
                    isRangeStart &&
                      rangeValue?.to &&
                      "rounded-l-full bg-neutral-100 dark:bg-zinc-800",
                    isRangeEnd &&
                      rangeValue?.from &&
                      "rounded-r-full bg-neutral-100 dark:bg-zinc-800"
                  )}
                >
                  <button
                    type="button"
                    disabled={isDisabled}
                    onClick={() => handleSelectDay(day)}
                    className={cn(
                      "flex size-8 cursor-pointer items-center justify-center rounded-full transition-all duration-150",
                      // Default non-selected
                      !isSingleSelected &&
                        !isRangeStart &&
                        !isRangeEnd &&
                        !isDayToday &&
                        "text-neutral-900 hover:bg-neutral-100 dark:text-zinc-100 dark:hover:bg-zinc-800",
                      // Today: clean circle outline with brand color
                      isDayToday &&
                        !isSingleSelected &&
                        !isRangeStart &&
                        !isRangeEnd &&
                        "border-brand text-brand dark:border-brand dark:text-brand-hover border-2 font-bold",
                      // Selected single or range endpoint: solid brand circle
                      (isSingleSelected || isRangeStart || isRangeEnd) &&
                        "bg-brand text-brand-foreground hover:bg-brand-hover font-bold shadow-xs",
                      // Range middle text
                      isRangeMiddle &&
                        "font-medium text-neutral-900 dark:text-zinc-100",
                      // Disabled
                      isDisabled &&
                        "cursor-not-allowed opacity-30 hover:bg-transparent"
                    )}
                  >
                    {format(day, "d")}
                  </button>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

/* ==========================================================================
   Year & Month Wheel / List Picker (Design Reference 2)
   ========================================================================== */

interface YearMonthPickerProps {
  currentDate: Date;
  onSelect: (date: Date) => void;
}

export function YearMonthPicker({
  currentDate,
  onSelect,
}: YearMonthPickerProps) {
  const currentYear = getYear(currentDate);
  const currentMonthIdx = getMonth(currentDate);

  // Generate 5 entries (2 past, 1 active, 2 future) matching design reference
  const items = [
    {
      month: (currentMonthIdx + 10) % 12,
      year: currentYear - 2,
      opacity: "opacity-30 text-xs",
    },
    {
      month: (currentMonthIdx + 11) % 12,
      year: currentYear - 1,
      opacity: "opacity-60 text-xs font-medium",
    },
    { month: currentMonthIdx, year: currentYear, isActive: true },
    {
      month: (currentMonthIdx + 1) % 12,
      year: currentYear + 1,
      opacity: "opacity-60 text-xs font-medium",
    },
    {
      month: (currentMonthIdx + 2) % 12,
      year: currentYear + 2,
      opacity: "opacity-30 text-xs",
    },
  ];

  return (
    <div className="mt-6 flex flex-col items-center justify-center space-y-2 py-4">
      {items.map((item, idx) => {
        const monthName = MONTH_NAMES[item.month];
        const dateForEntry = setYear(
          setMonth(new Date(), item.month),
          item.year
        );

        if (item.isActive) {
          return (
            <div
              key={idx}
              className="bg-brand-muted text-brand border-brand-border/40 flex w-full items-center justify-between rounded-2xl border px-6 py-3 text-sm font-bold shadow-2xs"
            >
              <span>{monthName}</span>
              <span>{item.year}</span>
            </div>
          );
        }

        return (
          <button
            key={idx}
            type="button"
            onClick={() => onSelect(dateForEntry)}
            className={cn(
              "flex w-full cursor-pointer items-center justify-between px-6 py-2 text-neutral-600 transition-colors hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-white",
              item.opacity
            )}
          >
            <span>{monthName}</span>
            <span>{item.year}</span>
          </button>
        );
      })}
    </div>
  );
}

/* ==========================================================================
   Date Range Picker (Dual Month + Quick Presets Sidebar - Design Reference 3)
   ========================================================================== */

const DEFAULT_PRESETS = [
  {
    label: "1 Week ago",
    getValue: (): DateRange => ({
      from: subWeeks(new Date(), 1),
      to: new Date(),
    }),
  },
  {
    label: "1 Month ago",
    getValue: (): DateRange => ({
      from: subMonths(new Date(), 1),
      to: new Date(),
    }),
  },
  {
    label: "3 Month ago",
    getValue: (): DateRange => ({
      from: subMonths(new Date(), 3),
      to: new Date(),
    }),
  },
  {
    label: "6 Month ago",
    getValue: (): DateRange => ({
      from: subMonths(new Date(), 6),
      to: new Date(),
    }),
  },
  {
    label: "1 Year ago",
    getValue: (): DateRange => ({
      from: subYears(new Date(), 1),
      to: new Date(),
    }),
  },
];

export function DateRangePicker({
  value,
  defaultValue,
  onChange,
  className,
  showPresets = true,
  presets = DEFAULT_PRESETS,
}: DateRangePickerProps) {
  const [uncontrolledRange, setUncontrolledRange] = React.useState<DateRange>(
    () => {
      return (
        defaultValue || {
          from: subDays(new Date(), 7),
          to: new Date(),
        }
      );
    }
  );
  const range = value !== undefined ? value : uncontrolledRange;

  const [leftMonthDate, setLeftMonthDate] = React.useState<Date>(() => {
    return range.from ? startOfMonth(range.from) : startOfMonth(new Date());
  });

  const rightMonthDate = addMonths(leftMonthDate, 1);

  const handlePrevMonth = () => {
    setLeftMonthDate((prev) => subMonths(prev, 1));
  };

  const handleNextMonth = () => {
    setLeftMonthDate((prev) => addMonths(prev, 1));
  };

  const handleDayClick = (day: Date) => {
    let newRange: DateRange;
    if (!range.from || (range.from && range.to)) {
      newRange = { from: day, to: undefined };
    } else {
      if (isBefore(day, range.from)) {
        newRange = { from: day, to: range.from };
      } else {
        newRange = { from: range.from, to: day };
      }
    }

    if (value === undefined) {
      setUncontrolledRange(newRange);
    }
    onChange?.(newRange);
  };

  const handlePresetSelect = (preset: {
    label: string;
    getValue: () => DateRange;
  }) => {
    const newRange = preset.getValue();
    if (value === undefined) {
      setUncontrolledRange(newRange);
    }
    if (newRange.from) {
      setLeftMonthDate(startOfMonth(newRange.from));
    }
    onChange?.(newRange);
  };

  return (
    <div
      className={cn(
        "inline-flex flex-col rounded-3xl border border-neutral-200/90 bg-white shadow-md transition-colors select-none md:flex-row dark:border-zinc-800/90 dark:bg-zinc-950",
        className
      )}
    >
      {/* Dual Month Calendar View */}
      <div className="flex flex-col gap-6 p-6 sm:flex-row sm:gap-8">
        {/* Left Month */}
        <MonthCalendarGrid
          monthDate={leftMonthDate}
          range={range}
          onDayClick={handleDayClick}
          onPrevMonth={handlePrevMonth}
          showPrevButton={true}
          showNextButton={false}
        />

        {/* Right Month */}
        <MonthCalendarGrid
          monthDate={rightMonthDate}
          range={range}
          onDayClick={handleDayClick}
          onNextMonth={handleNextMonth}
          showPrevButton={false}
          showNextButton={true}
        />
      </div>

      {/* Presets Sidebar */}
      {showPresets && (
        <div className="flex flex-col justify-between border-t border-neutral-200/80 p-5 md:w-48 md:border-t-0 md:border-l dark:border-zinc-800">
          <div className="space-y-1">
            {presets.map((preset) => (
              <button
                key={preset.label}
                type="button"
                onClick={() => handlePresetSelect(preset)}
                className="w-full cursor-pointer rounded-xl px-3 py-2 text-left text-xs font-medium text-neutral-700 transition-colors hover:bg-neutral-100 hover:text-neutral-950 active:scale-[0.98] dark:text-zinc-300 dark:hover:bg-zinc-800/60 dark:hover:text-white"
              >
                {preset.label}
              </button>
            ))}
          </div>

          <div className="pt-2">
            <button
              type="button"
              className="w-full cursor-pointer rounded-xl px-3 py-2 text-left text-xs font-medium text-neutral-500 hover:bg-neutral-100 hover:text-neutral-900 dark:text-zinc-400 dark:hover:bg-zinc-800"
            >
              Option...
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

/* ==========================================================================
   Internal Month Calendar Grid for Dual Month Range Picker
   ========================================================================== */

interface MonthCalendarGridProps {
  monthDate: Date;
  range: DateRange;
  onDayClick: (day: Date) => void;
  onPrevMonth?: () => void;
  onNextMonth?: () => void;
  showPrevButton?: boolean;
  showNextButton?: boolean;
}

function MonthCalendarGrid({
  monthDate,
  range,
  onDayClick,
  onPrevMonth,
  onNextMonth,
  showPrevButton,
  showNextButton,
}: MonthCalendarGridProps) {
  const monthStart = startOfMonth(monthDate);
  const monthEnd = endOfMonth(monthDate);
  const startDate = startOfWeek(monthStart, { weekStartsOn: 1 });
  const endDate = endOfWeek(monthEnd, { weekStartsOn: 1 });
  const days = eachDayOfInterval({ start: startDate, end: endDate });

  return (
    <div className="w-[280px]">
      {/* Header */}
      <div className="flex items-center justify-between pb-3">
        {showPrevButton ? (
          <button
            type="button"
            onClick={onPrevMonth}
            className="flex size-8 cursor-pointer items-center justify-center rounded-full text-neutral-600 transition-colors hover:bg-neutral-100 dark:text-zinc-400 dark:hover:bg-zinc-800"
            aria-label="Previous month"
          >
            <ChevronLeft className="size-4" />
          </button>
        ) : (
          <div className="size-8" />
        )}

        <div className="flex items-center gap-1.5 text-sm font-bold text-neutral-900 dark:text-white">
          <span>{format(monthDate, "MMMM yyyy")}</span>
          <ChevronDown className="size-3.5 text-neutral-400" />
        </div>

        {showNextButton ? (
          <button
            type="button"
            onClick={onNextMonth}
            className="flex size-8 cursor-pointer items-center justify-center rounded-full text-neutral-600 transition-colors hover:bg-neutral-100 dark:text-zinc-400 dark:hover:bg-zinc-800"
            aria-label="Next month"
          >
            <ChevronRight className="size-4" />
          </button>
        ) : (
          <div className="size-8" />
        )}
      </div>

      {/* Weekday Header */}
      <div className="grid grid-cols-7 text-center text-xs font-medium text-neutral-500 dark:text-zinc-400">
        {WEEKDAYS.map((day) => (
          <div key={day} className="py-1">
            {day}
          </div>
        ))}
      </div>

      {/* Days */}
      <div className="mt-2 grid grid-cols-7 gap-y-1">
        {days.map((day) => {
          const isCurrentMonth = isSameMonth(day, monthDate);
          const isDayToday = isToday(day);

          const isRangeStart = range?.from && isSameDay(day, range.from);
          const isRangeEnd = range?.to && isSameDay(day, range.to);
          const isRangeMiddle =
            range?.from &&
            range?.to &&
            isWithinInterval(day, {
              start: range.from,
              end: range.to,
            }) &&
            !isRangeStart &&
            !isRangeEnd;

          if (!isCurrentMonth) {
            return (
              <div
                key={day.toISOString()}
                className="flex size-8 items-center justify-center"
              />
            );
          }

          return (
            <div
              key={day.toISOString()}
              className={cn(
                "relative flex size-8 items-center justify-center p-0 text-xs font-normal",
                isRangeMiddle && "bg-neutral-100 dark:bg-zinc-800",
                isRangeStart &&
                  range?.to &&
                  "rounded-l-full bg-neutral-100 dark:bg-zinc-800",
                isRangeEnd &&
                  range?.from &&
                  "rounded-r-full bg-neutral-100 dark:bg-zinc-800"
              )}
            >
              <button
                type="button"
                onClick={() => onDayClick(day)}
                className={cn(
                  "flex size-7.5 cursor-pointer items-center justify-center rounded-full transition-all duration-150",
                  // Default
                  !isRangeStart &&
                    !isRangeEnd &&
                    !isDayToday &&
                    "text-neutral-900 hover:bg-neutral-100 dark:text-zinc-100 dark:hover:bg-zinc-800",
                  // Today (outline circle)
                  isDayToday &&
                    !isRangeStart &&
                    !isRangeEnd &&
                    "border-brand text-brand dark:border-brand dark:text-brand-hover border-2 font-bold",
                  // Range Start / End (solid brand circle)
                  (isRangeStart || isRangeEnd) &&
                    "bg-brand text-brand-foreground hover:bg-brand-hover font-bold shadow-xs",
                  // Range Middle Text
                  isRangeMiddle &&
                    "font-medium text-neutral-900 dark:text-zinc-100"
                )}
              >
                {format(day, "d")}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}
