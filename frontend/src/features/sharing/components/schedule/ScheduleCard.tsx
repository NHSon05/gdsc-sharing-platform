"use client";

import React from "react";
import { format } from "date-fns";
import type { ScheduleResponse } from "../../types/sharing.types";
import { StatusBadge } from "../common/StatusBadge";
import { DeliveryModeBadge } from "../common/DeliveryModeBadge";
import { MapPin, Video, ArrowRight } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { useTranslation } from "@/core/i18n/i18n.context";

import { useScheduleRealtime } from "../../hooks/use-schedule-realtime";

interface ScheduleCardProps {
  schedule: ScheduleResponse;
  onClick?: (schedule: ScheduleResponse) => void;
  layout?: "horizontal" | "vertical";
  className?: string;
}

export function ScheduleCard({
  schedule,
  onClick,
  layout = "horizontal",
  className,
}: ScheduleCardProps) {
  const { t } = useTranslation();
  const startDate = new Date(schedule.startsAtUtc);
  const endDate = new Date(schedule.endsAtUtc);
  const realtimeState = useScheduleRealtime(schedule);

  const mainPresenter = schedule.presenters[0];
  const presenterInitials = mainPresenter?.fullName
    ? mainPresenter.fullName
        .split(" ")
        .map((n) => n[0])
        .join("")
        .slice(0, 2)
        .toUpperCase()
    : "GD";

  const durationMinutes = Math.max(
    0,
    Math.round((endDate.getTime() - startDate.getTime()) / (1000 * 60))
  );
  const hours = Math.floor(durationMinutes / 60);
  const mins = durationMinutes % 60;
  const durationText =
    hours > 0 ? (mins > 0 ? `${hours}h ${mins}m` : `${hours}h`) : `${mins}m`;

  if (layout === "vertical") {
    return (
      <div
        onClick={() => onClick?.(schedule)}
        className={`group flex cursor-pointer flex-col justify-between rounded-2xl border bg-white p-5 shadow-2xs transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md dark:bg-zinc-900/80 ${
          realtimeState?.isLive
            ? "border-rose-400 ring-2 shadow-rose-500/5 ring-rose-500/20 dark:border-rose-700 dark:ring-rose-500/30"
            : "hover:border-brand/40 dark:hover:border-brand/40 border-neutral-200 dark:border-zinc-800"
        } ${className ?? ""}`}
      >
        <div>
          {/* Top Badges */}
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-100 pb-3 dark:border-zinc-800/80">
            <div className="flex items-center gap-2">
              <DeliveryModeBadge mode={schedule.deliveryMode} />
              <Badge
                variant="secondary"
                className="text-[10px] font-semibold tracking-wider uppercase"
              >
                {schedule.sharingType}
              </Badge>
            </div>

            {/* Real-time Status Badges */}
            {realtimeState?.isLive ? (
              <Badge className="inline-flex animate-pulse items-center gap-1.5 border-rose-300 bg-rose-50 px-2.5 py-0.5 text-[11px] font-bold text-rose-600 dark:border-rose-800 dark:bg-rose-950/60 dark:text-rose-300">
                <span className="size-1.5 animate-ping rounded-full bg-rose-500" />
                <span>LIVE</span>
              </Badge>
            ) : realtimeState?.isUpcoming && realtimeState.relativeTimeText ? (
              <div className="flex items-center gap-1.5">
                <span className="rounded-md border border-blue-200/80 bg-blue-50/80 px-2 py-0.5 text-[10px] font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
                  {realtimeState.relativeTimeText}
                </span>
                <StatusBadge status={schedule.status} />
              </div>
            ) : (
              <StatusBadge status={schedule.status} />
            )}
          </div>

          {/* Title & Description */}
          <div className="mt-3 space-y-1.5">
            <h3 className="group-hover:text-brand dark:group-hover:text-brand line-clamp-2 text-base font-bold text-neutral-900 transition-colors dark:text-white">
              {schedule.title}
            </h3>
            {schedule.description && (
              <p className="line-clamp-2 text-xs leading-relaxed text-neutral-500 dark:text-zinc-400">
                {schedule.description}
              </p>
            )}
          </div>
        </div>

        {/* Footer Info */}
        <div className="mt-5 space-y-2 border-t border-neutral-100 pt-3 text-xs text-neutral-600 dark:border-zinc-800/80 dark:text-zinc-400">
          <div className="flex items-center gap-2">
            <span className="font-semibold text-neutral-800 dark:text-zinc-200">
              {format(startDate, "EEE, MMM d, yyyy")} •{" "}
              {format(startDate, "HH:mm")} - {format(endDate, "HH:mm")} (
              {schedule.timeZoneId || "UTC"})
            </span>
          </div>

          {schedule.deliveryMode !== "Online" && schedule.location && (
            <div className="flex items-center gap-2">
              <MapPin className="size-3.5 shrink-0 text-neutral-400" />
              <span className="truncate">{schedule.location}</span>
            </div>
          )}

          {schedule.deliveryMode !== "Offline" && (
            <div className="flex items-center gap-2">
              <Video className="size-3.5 shrink-0 text-sky-500" />
              <span className="truncate">
                {schedule.meetingUrl ? (
                  <span className="font-medium text-sky-600 dark:text-sky-400">
                    Meeting Link Available
                  </span>
                ) : (
                  <span className="text-neutral-400 italic">
                    Link restricted to audience
                  </span>
                )}
              </span>
            </div>
          )}

          {mainPresenter && (
            <div className="flex items-center justify-between pt-1 text-[11px] text-neutral-500 dark:text-zinc-400">
              <div className="flex min-w-0 items-center gap-1.5">
                <div className="bg-brand/10 text-brand flex size-5 items-center justify-center rounded-full text-[9px] font-bold uppercase">
                  {presenterInitials}
                </div>
                <span className="truncate font-medium text-neutral-700 dark:text-zinc-300">
                  {mainPresenter.fullName}
                </span>
                <span className="text-neutral-400">({mainPresenter.role})</span>
              </div>
            </div>
          )}
        </div>
      </div>
    );
  }

  // Horizontal layout for List View
  return (
    <div
      onClick={() => onClick?.(schedule)}
      className={`group flex cursor-pointer flex-col rounded-2xl border bg-white p-5 shadow-2xs transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md md:flex-row md:items-stretch dark:bg-zinc-900/80 ${
        realtimeState?.isLive
          ? "border-rose-400 ring-2 shadow-rose-500/5 ring-rose-500/20 dark:border-rose-700 dark:ring-rose-500/30"
          : "hover:border-brand/40 dark:hover:border-brand/40 border-neutral-200/80 dark:border-zinc-800/80"
      } ${className ?? ""}`}
    >
      {/* Left Column: Time & Delivery Mode */}
      <div className="flex shrink-0 flex-col justify-between space-y-3 border-b border-neutral-100 pb-3 md:w-56 md:border-r md:border-b-0 md:pr-6 md:pb-0 dark:border-zinc-800/80">
        <div>
          <div className="flex items-baseline gap-1.5 text-base font-bold tracking-tight text-neutral-900 sm:text-lg dark:text-white">
            <span>{format(startDate, "HH:mm")}</span>
            <span className="text-sm font-normal text-neutral-400">–</span>
            <span>{format(endDate, "HH:mm")}</span>
          </div>
          <div className="mt-1 flex items-center gap-1.5 text-xs text-neutral-400 dark:text-zinc-500">
            <span>{durationText}</span>
            <span>•</span>
            <span className="truncate">{schedule.timeZoneId || "GMT+7"}</span>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <DeliveryModeBadge mode={schedule.deliveryMode} />
        </div>
      </div>

      {/* Middle Column: Title, Description, Tags, Presenter */}
      <div className="flex min-w-0 flex-1 flex-col justify-between space-y-3 py-3 md:px-6 md:py-0">
        <div>
          {/* Top Badges Row */}
          <div className="flex flex-wrap items-center gap-2">
            <Badge
              variant="secondary"
              className="text-[10px] font-bold tracking-wider uppercase"
            >
              {schedule.sharingType}
            </Badge>

            {realtimeState?.isLive ? (
              <Badge className="inline-flex animate-pulse items-center gap-1.5 border-rose-300 bg-rose-50 px-2.5 py-0.5 text-[11px] font-bold text-rose-600 dark:border-rose-800 dark:bg-rose-950/60 dark:text-rose-300">
                <span className="size-1.5 animate-ping rounded-full bg-rose-500" />
                <span>LIVE</span>
              </Badge>
            ) : realtimeState?.isUpcoming && realtimeState.relativeTimeText ? (
              <span className="rounded-md border border-blue-200/80 bg-blue-50/80 px-2 py-0.5 text-[10px] font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
                {realtimeState.relativeTimeText}
              </span>
            ) : null}

            <StatusBadge status={schedule.status} />
          </div>

          {/* Title & Description */}
          <h3 className="group-hover:text-brand dark:group-hover:text-brand mt-2 text-base font-bold text-neutral-900 transition-colors sm:text-lg dark:text-white">
            {schedule.title}
          </h3>
          {schedule.description && (
            <p className="mt-1 line-clamp-2 text-xs leading-relaxed text-neutral-500 sm:text-sm dark:text-zinc-400">
              {schedule.description}
            </p>
          )}
        </div>

        {/* Footer Meta Row: Location & Presenters */}
        <div className="flex flex-wrap items-center gap-x-5 gap-y-2 pt-1 text-xs text-neutral-600 dark:text-zinc-400">
          {schedule.deliveryMode !== "Online" && schedule.location && (
            <div className="flex items-center gap-1.5">
              <MapPin className="size-3.5 shrink-0 text-neutral-400" />
              <span className="max-w-[200px] truncate sm:max-w-xs">
                {schedule.location}
              </span>
            </div>
          )}

          {schedule.deliveryMode !== "Offline" && (
            <div className="flex items-center gap-1.5">
              <Video className="size-3.5 shrink-0 text-sky-500" />
              <span className="truncate">
                {schedule.meetingUrl ? (
                  <span className="font-medium text-sky-600 dark:text-sky-400">
                    Meeting Link Available
                  </span>
                ) : (
                  <span className="text-neutral-400 italic">
                    Link restricted to audience
                  </span>
                )}
              </span>
            </div>
          )}

          {mainPresenter && (
            <div className="flex items-center gap-2">
              <div className="bg-brand/10 text-brand flex size-5 items-center justify-center rounded-full text-[9px] font-bold uppercase">
                {presenterInitials}
              </div>
              <span className="font-medium text-neutral-700 dark:text-zinc-300">
                {mainPresenter.fullName}
              </span>
              <span className="text-[11px] text-neutral-400">
                ({mainPresenter.role})
              </span>
              {schedule.presenters.length > 1 && (
                <span className="text-[11px] text-neutral-400">
                  +{schedule.presenters.length - 1} more
                </span>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Right Column: Action Affordance */}
      <div className="flex shrink-0 items-center justify-between border-t border-neutral-100 pt-3 md:w-32 md:flex-col md:items-end md:justify-center md:border-t-0 md:pt-0 md:pl-4 dark:border-zinc-800/80">
        <div className="group-hover:bg-brand group-hover:border-brand flex items-center gap-1.5 rounded-xl border border-neutral-200/80 bg-neutral-50/80 px-3 py-1.5 text-xs font-semibold text-neutral-700 transition-all duration-150 group-hover:text-white dark:border-zinc-800 dark:bg-zinc-800/80 dark:text-zinc-300">
          <span>{t("common.viewDetail") || "Xem chi tiết"}</span>
          <ArrowRight className="size-3.5 transition-transform duration-150 group-hover:translate-x-0.5" />
        </div>
      </div>
    </div>
  );
}
