"use client";

import React, { useState, useMemo } from "react";
import { useSearchParams } from "next/navigation";
import {
  ScheduleListView,
  ScheduleCalendarView,
  ScheduleFilterSidebar,
  ScheduleDetailDialog,
  ScheduleFormDialog,
  useSchedulesQuery,
  useScheduleDetailQuery,
} from "@/features/sharing";
import type {
  ScheduleResponse,
  DeliveryMode,
  SharingType,
} from "@/features/sharing";
import { Calendar as CalendarIcon, List as ListIcon } from "lucide-react";

const ALL_DELIVERY_MODES: DeliveryMode[] = ["Offline", "Online", "Hybrid"];
const ALL_SHARING_TYPES: SharingType[] = [
  "TechTalk",
  "Workshop",
  "PanelDiscussion",
  "InternalSharing",
];

export default function SchedulePage() {
  const searchParams = useSearchParams();
  const initialId = searchParams.get("id");

  const [viewMode, setViewMode] = useState<"calendar" | "list">("calendar");
  const [selectedDeliveryModes, setSelectedDeliveryModes] =
    useState<DeliveryMode[]>(ALL_DELIVERY_MODES);
  const [selectedSharingTypes, setSelectedSharingTypes] =
    useState<SharingType[]>(ALL_SHARING_TYPES);

  // Fetch real schedules from API
  const {
    data: schedulePage,
    isLoading,
    refetch,
  } = useSchedulesQuery({
    pageSize: 100,
  });

  const allSchedules = useMemo(
    () => schedulePage?.items ?? [],
    [schedulePage?.items]
  );

  // Filter schedules based on checkbox selections
  const filteredSchedules = useMemo(() => {
    return allSchedules.filter((item) => {
      const matchesDelivery =
        selectedDeliveryModes.length === 0 ||
        selectedDeliveryModes.includes(item.deliveryMode);

      const matchesType =
        selectedSharingTypes.length === 0 ||
        selectedSharingTypes.includes(item.sharingType);

      return matchesDelivery && matchesType;
    });
  }, [allSchedules, selectedDeliveryModes, selectedSharingTypes]);

  // Counts for each mode and type
  const filterCounts = useMemo(() => {
    const deliveryCounts: Record<DeliveryMode, number> = {
      Offline: 0,
      Online: 0,
      Hybrid: 0,
    };
    const typeCounts: Record<SharingType, number> = {
      TechTalk: 0,
      Workshop: 0,
      PanelDiscussion: 0,
      InternalSharing: 0,
    };

    allSchedules.forEach((s) => {
      if (deliveryCounts[s.deliveryMode] !== undefined) {
        deliveryCounts[s.deliveryMode]++;
      }
      if (typeCounts[s.sharingType] !== undefined) {
        typeCounts[s.sharingType]++;
      }
    });

    return { deliveryModes: deliveryCounts, sharingTypes: typeCounts };
  }, [allSchedules]);

  const handleToggleDeliveryMode = (mode: DeliveryMode) => {
    setSelectedDeliveryModes((prev) =>
      prev.includes(mode) ? prev.filter((m) => m !== mode) : [...prev, mode]
    );
  };

  const handleToggleSharingType = (type: SharingType) => {
    setSelectedSharingTypes((prev) =>
      prev.includes(type) ? prev.filter((t) => t !== type) : [...prev, type]
    );
  };

  const handleResetFilters = () => {
    setSelectedDeliveryModes(ALL_DELIVERY_MODES);
    setSelectedSharingTypes(ALL_SHARING_TYPES);
  };

  // Dialog states
  const [selectedSchedule, setSelectedSchedule] =
    useState<ScheduleResponse | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] =
    useState<ScheduleResponse | null>(null);

  // Auto-open schedule if ?id=xyz in searchParams
  const { data: linkedSchedule } = useScheduleDetailQuery(
    initialId || "",
    Boolean(initialId)
  );

  const activeSchedule =
    selectedSchedule || (initialId ? linkedSchedule || null : null);

  const handleCreateNew = () => {
    setEditingSchedule(null);
    setIsFormOpen(true);
  };

  const handleEdit = (schedule: ScheduleResponse) => {
    setEditingSchedule(schedule);
    setIsFormOpen(true);
  };

  const viewModeToggle = (
    <div className="flex items-center gap-1 rounded-full border border-neutral-200 bg-white shadow-2xs dark:border-zinc-800 dark:bg-zinc-900">
      <button
        type="button"
        onClick={() => setViewMode("calendar")}
        className={`flex cursor-pointer items-center gap-1.5 rounded-full px-3 py-2 text-xs font-semibold transition-all ${
          viewMode === "calendar"
            ? "bg-brand text-white shadow-xs"
            : "text-neutral-600 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
        }`}
      >
        <CalendarIcon className="size-4" />
      </button>
      <button
        type="button"
        onClick={() => setViewMode("list")}
        className={`flex cursor-pointer items-center gap-1.5 rounded-full px-3 py-2 text-xs font-semibold transition-all ${
          viewMode === "list"
            ? "bg-brand text-white shadow-xs"
            : "text-neutral-600 hover:text-neutral-900 dark:text-zinc-400 dark:hover:text-zinc-100"
        }`}
      >
        <ListIcon className="size-4" />
      </button>
    </div>
  );

  return (
    <div className="mx-auto">
      {/* Main View Area */}
      {viewMode === "calendar" ? (
        <ScheduleCalendarView
          schedules={filteredSchedules}
          onSelectSchedule={setSelectedSchedule}
          onAddSchedule={handleCreateNew}
          headerActions={viewModeToggle}
          sidebarFilters={
            <ScheduleFilterSidebar
              selectedDeliveryModes={selectedDeliveryModes}
              onToggleDeliveryMode={handleToggleDeliveryMode}
              selectedSharingTypes={selectedSharingTypes}
              onToggleSharingType={handleToggleSharingType}
              onResetFilters={handleResetFilters}
              counts={filterCounts}
            />
          }
        />
      ) : (
        <ScheduleListView
          schedules={filteredSchedules}
          isLoading={isLoading}
          onSelectSchedule={setSelectedSchedule}
          onAddSchedule={handleCreateNew}
          headerActions={viewModeToggle}
          sidebarFilters={
            <ScheduleFilterSidebar
              selectedDeliveryModes={selectedDeliveryModes}
              onToggleDeliveryMode={handleToggleDeliveryMode}
              selectedSharingTypes={selectedSharingTypes}
              onToggleSharingType={handleToggleSharingType}
              onResetFilters={handleResetFilters}
              counts={filterCounts}
            />
          }
        />
      )}

      {/* Event Detail Dialog */}
      <ScheduleDetailDialog
        schedule={activeSchedule}
        isOpen={Boolean(activeSchedule)}
        onClose={() => setSelectedSchedule(null)}
        onEdit={handleEdit}
        canManage={true}
      />

      {/* Form Dialog for Create / Edit */}
      {isFormOpen && (
        <ScheduleFormDialog
          isOpen={isFormOpen}
          onClose={() => {
            setIsFormOpen(false);
            setEditingSchedule(null);
          }}
          initialData={editingSchedule}
          onSuccess={() => {
            refetch();
          }}
        />
      )}
    </div>
  );
}
