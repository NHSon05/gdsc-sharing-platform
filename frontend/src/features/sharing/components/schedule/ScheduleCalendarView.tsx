"use client";

import React, { useMemo } from "react";
import {
  EventCalendar,
  type CalendarEvent,
  type EventColor,
} from "@/components/ui/event-calendar";
import type { ScheduleResponse } from "../../types/sharing.types";

import { useCurrentTime } from "../../hooks/use-schedule-realtime";

interface ScheduleCalendarViewProps {
  schedules: ScheduleResponse[];
  onSelectSchedule: (schedule: ScheduleResponse) => void;
  onAddSchedule?: () => void;
  sidebarFilters?: React.ReactNode;
  headerActions?: React.ReactNode;
  className?: string;
}

export function ScheduleCalendarView({
  schedules,
  onSelectSchedule,
  onAddSchedule,
  sidebarFilters,
  headerActions,
  className,
}: ScheduleCalendarViewProps) {
  const now = useCurrentTime(10_000);

  // Map backend schedules to EventCalendar format
  const { events, scheduleMap } = useMemo(() => {
    const map = new Map<string, ScheduleResponse>();
    const currentTime = now.getTime();

    const calendarEvents: CalendarEvent[] = schedules.map((item) => {
      map.set(item.id, item);

      const start = new Date(item.startsAtUtc);
      const end = new Date(item.endsAtUtc);
      const isLive =
        item.status !== "Cancelled" &&
        item.status !== "Completed" &&
        currentTime >= start.getTime() &&
        currentTime <= end.getTime();

      let color: EventColor = "blue";
      if (item.deliveryMode === "Online") color = "cyan";
      if (item.deliveryMode === "Hybrid") color = "purple";
      if (item.status === "Cancelled") color = "rose";
      if (item.status === "InProgress" || isLive) color = "amber";

      return {
        id: item.id,
        title: isLive ? `🔴 LIVE: ${item.title}` : item.title,
        startDate: start,
        endDate: end,
        location: item.location || (item.meetingUrl ? "Online" : undefined),
        color,
        description: item.description || undefined,
        category: item.sharingType,
      };
    });

    return { events: calendarEvents, scheduleMap: map };
  }, [schedules, now]);

  const handleEventClick = (event: CalendarEvent) => {
    const original = scheduleMap.get(event.id);
    if (original) {
      onSelectSchedule(original);
    }
  };

  return (
    <div className={`overflow-hidden rounded-2xl ${className ?? ""}`}>
      <EventCalendar
        events={events}
        onEventClick={handleEventClick}
        onAddEvent={onAddSchedule}
        sidebarFilters={sidebarFilters}
        headerActions={headerActions}
      />
    </div>
  );
}
