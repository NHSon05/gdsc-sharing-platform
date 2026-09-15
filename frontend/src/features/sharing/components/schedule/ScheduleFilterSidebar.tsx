"use client";

import React from "react";
import { useTranslation } from "@/core/i18n/i18n.context";
import type { DeliveryMode, SharingType } from "../../types/sharing.types";
import { Check, RotateCcw } from "lucide-react";
import { cn } from "@/lib/utils";

interface ScheduleFilterSidebarProps {
  selectedDeliveryModes: DeliveryMode[];
  onToggleDeliveryMode: (mode: DeliveryMode) => void;
  selectedSharingTypes: SharingType[];
  onToggleSharingType: (type: SharingType) => void;
  onResetFilters?: () => void;
  counts?: {
    deliveryModes?: Record<DeliveryMode, number>;
    sharingTypes?: Record<SharingType, number>;
  };
  className?: string;
}

export function ScheduleFilterSidebar({
  selectedDeliveryModes,
  onToggleDeliveryMode,
  selectedSharingTypes,
  onToggleSharingType,
  onResetFilters,
  counts,
  className,
}: ScheduleFilterSidebarProps) {
  const { t } = useTranslation();

  const deliveryModes: { id: DeliveryMode; label: string; dotColor: string }[] =
    [
      {
        id: "Offline",
        label: t("sharing.deliveryOffline"),
        dotColor: "bg-emerald-500",
      },
      {
        id: "Online",
        label: t("sharing.deliveryOnline"),
        dotColor: "bg-sky-500",
      },
      {
        id: "Hybrid",
        label: t("sharing.deliveryHybrid"),
        dotColor: "bg-purple-500",
      },
    ];

  const sharingTypes: { id: SharingType; label: string; dotColor: string }[] = [
    { id: "TechTalk", label: "TechTalk", dotColor: "bg-blue-500" },
    { id: "Workshop", label: "Workshop", dotColor: "bg-amber-500" },
    {
      id: "PanelDiscussion",
      label: "Panel Discussion",
      dotColor: "bg-pink-500",
    },
    {
      id: "InternalSharing",
      label: "Internal Sharing",
      dotColor: "bg-teal-500",
    },
  ];

  const isFiltered =
    selectedDeliveryModes.length < deliveryModes.length ||
    selectedSharingTypes.length < sharingTypes.length;

  return (
    <div
      className={cn(
        "space-y-6 border-t border-neutral-200/70 pt-5 select-none dark:border-zinc-800/70",
        className
      )}
    >
      {/* 1. Delivery Mode Checkboxes */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 text-sm font-bold tracking-wider text-neutral-800 dark:text-zinc-200">
            <span>{t("sharing.deliveryMode")}</span>
          </div>
          {selectedDeliveryModes.length < deliveryModes.length && (
            <span className="text-brand text-[11px] font-semibold">
              {selectedDeliveryModes.length}/{deliveryModes.length}
            </span>
          )}
        </div>

        <div className="space-y-2 pt-0.5">
          {deliveryModes.map((mode) => {
            const isChecked = selectedDeliveryModes.includes(mode.id);
            const count = counts?.deliveryModes?.[mode.id];

            return (
              <label
                key={mode.id}
                onClick={() => onToggleDeliveryMode(mode.id)}
                className="group flex cursor-pointer items-center justify-between rounded-xl px-1.5 py-1 text-xs font-medium text-neutral-700 transition-colors hover:bg-neutral-100/60 dark:text-zinc-300 dark:hover:bg-zinc-800/60 dark:hover:text-white"
              >
                <div className="flex items-center gap-2.5">
                  <div
                    className={cn(
                      "flex size-4 items-center justify-center rounded-md border transition-all duration-150",
                      isChecked
                        ? "border-brand bg-brand text-white shadow-2xs"
                        : "border-neutral-300 bg-white group-hover:border-neutral-400 dark:border-zinc-700 dark:bg-zinc-900"
                    )}
                  >
                    {isChecked && <Check className="size-3 stroke-3" />}
                  </div>
                  <span className="flex items-center gap-1.5">
                    <span
                      className={cn("size-2 rounded-full", mode.dotColor)}
                    />
                    <span>{mode.label}</span>
                  </span>
                </div>

                {count !== undefined && (
                  <span className="rounded-full bg-neutral-100 px-1.5 py-0.5 text-[10px] font-semibold text-neutral-500 dark:bg-zinc-800 dark:text-zinc-400">
                    {count}
                  </span>
                )}
              </label>
            );
          })}
        </div>
      </div>

      {/* 2. Sharing Type Checkboxes */}
      <div className="space-y-3 border-t border-neutral-200/50 pt-5 dark:border-zinc-800/50">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 text-sm font-bold tracking-wider text-neutral-800 dark:text-zinc-200">
            <span>{t("sharing.sharingType")}</span>
          </div>
          {selectedSharingTypes.length < sharingTypes.length && (
            <span className="text-brand text-[11px] font-semibold">
              {selectedSharingTypes.length}/{sharingTypes.length}
            </span>
          )}
        </div>

        <div className="space-y-2 pt-0.5">
          {sharingTypes.map((type) => {
            const isChecked = selectedSharingTypes.includes(type.id);
            const count = counts?.sharingTypes?.[type.id];

            return (
              <label
                key={type.id}
                onClick={() => onToggleSharingType(type.id)}
                className="group flex cursor-pointer items-center justify-between rounded-xl px-1.5 py-1 text-xs font-medium text-neutral-700 transition-colors hover:bg-neutral-100/60 dark:text-zinc-300 dark:hover:bg-zinc-800/60 dark:hover:text-white"
              >
                <div className="flex items-center gap-2.5">
                  <div
                    className={cn(
                      "flex size-4 items-center justify-center rounded-md border transition-all duration-150",
                      isChecked
                        ? "border-brand bg-brand text-white shadow-2xs"
                        : "border-neutral-300 bg-white group-hover:border-neutral-400 dark:border-zinc-700 dark:bg-zinc-900"
                    )}
                  >
                    {isChecked && <Check className="size-3 stroke-3" />}
                  </div>
                  <span className="flex items-center gap-1.5">
                    <span
                      className={cn("size-2 rounded-full", type.dotColor)}
                    />
                    <span>{type.label}</span>
                  </span>
                </div>

                {count !== undefined && (
                  <span className="rounded-full bg-neutral-100 px-1.5 py-0.5 text-[10px] font-semibold text-neutral-500 dark:bg-zinc-800 dark:text-zinc-400">
                    {count}
                  </span>
                )}
              </label>
            );
          })}
        </div>
      </div>

      {/* 3. Reset Button when filtered */}
      {isFiltered && onResetFilters && (
        <button
          type="button"
          onClick={onResetFilters}
          className="text-brand hover:bg-brand-muted/20 flex w-full cursor-pointer items-center justify-center gap-1.5 rounded-xl border border-blue-200/80 bg-blue-50/50 py-2 text-xs font-semibold transition-colors dark:border-blue-900/60 dark:bg-blue-950/30 dark:text-blue-300"
        >
          <RotateCcw className="size-3" />
          <span>{t("sharing.resetFilters")}</span>
        </button>
      )}
    </div>
  );
}
