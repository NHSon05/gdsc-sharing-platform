"use client";

import React, { useState } from "react";
import {
  EventCalendar,
  type CalendarEvent,
} from "@/components/ui/event-calendar";
import { useTranslation } from "@/core/i18n/i18n.context";

export default function SchedulePage() {
  const { t } = useTranslation();
  const baseDate = new Date(2025, 11, 9); // Dec 09, 2025

  const sampleEvents: CalendarEvent[] = [
    {
      id: "1",
      title: "Council Chamber",
      startDate: new Date(2025, 11, 7, 1, 0), // Tue 01:00
      endDate: new Date(2025, 11, 7, 1, 45),
      location: "Location",
      color: "blue",
    },
    {
      id: "2",
      title: "Event Name",
      startDate: new Date(2025, 11, 7, 1, 50), // Tue 01:50
      endDate: new Date(2025, 11, 7, 2, 45),
      location: "Conference Cosmos",
      color: "cyan",
    },
    {
      id: "3",
      title: "Daily Standup",
      startDate: new Date(2025, 11, 11, 1, 0), // Sat 01:00
      endDate: new Date(2025, 11, 11, 1, 45),
      location: "Executive Exchange",
      color: "blue",
    },
    {
      id: "4",
      title: "1-on-1: Daphna <> Richard",
      startDate: new Date(2025, 11, 12, 1, 20), // Sun 01:20
      endDate: new Date(2025, 11, 12, 2, 15),
      location: "Idea Factory",
      color: "rose",
    },
    {
      id: "5",
      title: "Leadership Sync",
      startDate: new Date(2025, 11, 9, 2, 0), // Thu 02:00
      endDate: new Date(2025, 11, 9, 2, 50),
      location: "Meeting Mirage",
      color: "rose",
    },
    {
      id: "6",
      title: "Council Chamber",
      startDate: new Date(2025, 11, 6, 3, 0), // Mon 03:00
      endDate: new Date(2025, 11, 6, 3, 40),
      location: "Location",
      color: "blue",
    },
    {
      id: "7",
      title: "Product Demo",
      startDate: new Date(2025, 11, 6, 3, 45), // Mon 03:45
      endDate: new Date(2025, 11, 6, 4, 45),
      location: "Brainstorm Boulevard",
      color: "rose",
    },
    {
      id: "8",
      title: "1-on-1: Daphna <> Richard",
      startDate: new Date(2025, 11, 8, 3, 0), // Wed 03:00
      endDate: new Date(2025, 11, 8, 6, 30),
      location: "Power Playground",
      color: "amber",
    },
    {
      id: "9",
      title: "Usability Test: John S. from Mavenlink",
      startDate: new Date(2025, 11, 10, 4, 0), // Fri 04:00
      endDate: new Date(2025, 11, 10, 5, 30),
      location: "Discussion Den",
      color: "white",
    },
    {
      id: "10",
      title: "1-on-1: Daphna <> Richard",
      startDate: new Date(2025, 11, 10, 5, 45), // Fri 05:45
      endDate: new Date(2025, 11, 10, 6, 45),
      location: "Discussion Den",
      color: "white",
    },
    {
      id: "11",
      title: "Leadership Sync",
      startDate: new Date(2025, 11, 7, 6, 0), // Tue 06:00
      endDate: new Date(2025, 11, 7, 6, 20),
      location: "Brainstorm Bay",
      color: "white",
    },
    {
      id: "12",
      title: "Leadership Sync",
      startDate: new Date(2025, 11, 7, 6, 25), // Tue 06:25
      endDate: new Date(2025, 11, 7, 7, 15),
      location: "Brainstorm Bay",
      color: "cyan",
    },
    {
      id: "13",
      title: "Daily Standup",
      startDate: new Date(2025, 11, 9, 7, 0), // Thu 07:00
      endDate: new Date(2025, 11, 9, 7, 45),
      location: "Location",
      color: "white",
    },
    {
      id: "14",
      title: "1-on-1: Daphna <> Richard",
      startDate: new Date(2025, 11, 9, 7, 20), // Thu 07:20
      endDate: new Date(2025, 11, 9, 8, 10),
      location: "Conference Cosmos",
      color: "rose",
    },
    {
      id: "15",
      title: "Product Demo",
      startDate: new Date(2025, 11, 11, 8, 0), // Sat 08:00
      endDate: new Date(2025, 11, 11, 8, 45),
      location: "Room 102",
      color: "white",
    },
  ];

  const [selectedEvent, setSelectedEvent] = useState<CalendarEvent | null>(
    null
  );

  return (
    <div className="mx-auto space-y-6">
      {/* Main Event Calendar UI Component */}
      <EventCalendar
        date={baseDate}
        events={sampleEvents}
        onEventClick={(ev) => setSelectedEvent(ev)}
        onAddEvent={() => alert("Open Add Event Modal")}
        onSlotClick={(day, hour) =>
          alert(`Selected slot: ${day.toDateString()} at ${hour}:00`)
        }
      />

      {/* Event Details Toast / Notification */}
      {selectedEvent && (
        <div className="animate-in fade-in slide-in-from-bottom-3 fixed right-6 bottom-6 z-50 rounded-2xl border border-neutral-200 bg-white p-4 shadow-xl dark:border-zinc-800 dark:bg-zinc-900">
          <div className="flex items-start justify-between gap-4">
            <div>
              <span className="text-brand text-[11px] font-bold tracking-wider uppercase">
                {t("schedule.eventSelected")}
              </span>
              <h3 className="text-sm font-bold text-neutral-900 dark:text-white">
                {selectedEvent.title}
              </h3>
              <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
                {selectedEvent.startDate.toLocaleTimeString([], {
                  hour: "2-digit",
                  minute: "2-digit",
                })}{" "}
                - {selectedEvent.location}
              </p>
            </div>
            <button
              onClick={() => setSelectedEvent(null)}
              className="cursor-pointer text-xs font-semibold text-neutral-400 hover:text-neutral-900 dark:hover:text-white"
            >
              ✕
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
