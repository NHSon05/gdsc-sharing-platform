import React from "react";
import { format } from "date-fns";
import type { ScheduleResponse } from "../../types/sharing.types";
import { StatusBadge } from "../common/StatusBadge";
import { DeliveryModeBadge } from "../common/DeliveryModeBadge";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  Clock,
  MapPin,
  Video,
  Users,
  ExternalLink,
  BookOpen,
  AlertTriangle,
  FileEdit,
  XCircle,
} from "lucide-react";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

import { useScheduleRealtime } from "../../hooks/use-schedule-realtime";

interface ScheduleDetailDialogProps {
  schedule: ScheduleResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onEdit?: (schedule: ScheduleResponse) => void;
  onCancel?: (schedule: ScheduleResponse) => void;
  canManage?: boolean;
}

export function ScheduleDetailDialog({
  schedule,
  isOpen,
  onClose,
  onEdit,
  onCancel,
  canManage = false,
}: ScheduleDetailDialogProps) {
  const { t } = useTranslation();
  const realtimeState = useScheduleRealtime(schedule);

  if (!schedule) return null;

  const startDate = new Date(schedule.startsAtUtc);
  const endDate = new Date(schedule.endsAtUtc);

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-h-[90vh] max-w-2xl overflow-y-auto p-6">
        <DialogHeader>
          <div className="mb-2 flex flex-wrap items-center gap-2">
            <DeliveryModeBadge mode={schedule.deliveryMode} />
            <Badge
              variant="secondary"
              className="text-xs font-semibold uppercase"
            >
              {schedule.sharingType}
            </Badge>

            {realtimeState?.isLive ? (
              <Badge className="inline-flex animate-pulse items-center gap-1.5 border-rose-300 bg-rose-50 px-2.5 py-0.5 text-xs font-bold text-rose-600 dark:border-rose-800 dark:bg-rose-950/60 dark:text-rose-300">
                <span className="size-1.5 animate-ping rounded-full bg-rose-500" />
                <span>LIVE</span>
              </Badge>
            ) : realtimeState?.isUpcoming && realtimeState.relativeTimeText ? (
              <div className="flex items-center gap-1.5">
                <span className="rounded-md border border-blue-200/80 bg-blue-50/80 px-2 py-0.5 text-[11px] font-semibold text-blue-700 dark:border-blue-900/60 dark:bg-blue-950/50 dark:text-blue-300">
                  {realtimeState.relativeTimeText}
                </span>
                <StatusBadge status={schedule.status} />
              </div>
            ) : (
              <StatusBadge status={schedule.status} />
            )}
          </div>
          <DialogTitle className="text-xl font-bold text-neutral-900 dark:text-white">
            {schedule.title}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6 pt-2">
          {/* Cancellation Notice if Cancelled */}
          {schedule.status === "Cancelled" && schedule.cancellationReason && (
            <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-xs dark:border-rose-900/60 dark:bg-rose-950/40">
              <div className="flex items-center gap-2 font-bold text-rose-800 dark:text-rose-300">
                <AlertTriangle className="size-4" />
                <span>{t("sharing.cancellationReason")}</span>
              </div>
              <p className="mt-1 whitespace-pre-wrap text-rose-700 dark:text-rose-300">
                {schedule.cancellationReason}
              </p>
            </div>
          )}

          {/* Description */}
          {schedule.description && (
            <p className="text-sm leading-relaxed text-neutral-600 dark:text-zinc-300">
              {schedule.description}
            </p>
          )}

          {/* Timing & Location Details Card */}
          <div className="space-y-3 rounded-2xl border border-neutral-200 bg-neutral-50/70 p-4 dark:border-zinc-800 dark:bg-zinc-900/40">
            {/* Time */}
            <div className="flex items-start gap-3 text-sm">
              <Clock className="mt-0.5 size-6 shrink-0 text-neutral-400" />
              <div>
                <p className="font-semibold text-neutral-900 dark:text-white">
                  {format(startDate, "EEEE, MMMM d, yyyy")}
                </p>
                <p className="text-xs text-neutral-500 dark:text-zinc-400">
                  {format(startDate, "HH:mm")} - {format(endDate, "HH:mm")} (
                  {schedule.timeZoneId || "UTC"})
                </p>
              </div>
            </div>

            {/* Location (Offline / Hybrid) */}
            {schedule.deliveryMode !== "Online" && schedule.location && (
              <div className="flex items-start gap-3 text-sm">
                <MapPin className="mt-0.5 size-6 shrink-0 text-neutral-400" />
                <div>
                  <p className="font-semibold text-neutral-900 dark:text-white">
                    {schedule.location}
                  </p>
                </div>
              </div>
            )}

            {/* Meeting URL (Online / Hybrid) */}
            {schedule.deliveryMode !== "Offline" && (
              <div className="flex items-start gap-3 text-sm">
                <Video className="mt-0.5 size-6 shrink-0 text-sky-500" />
                <div>
                  <p className="font-semibold text-neutral-900 dark:text-white">
                    {t("sharing.meetingUrl")}
                  </p>
                  {schedule.meetingUrl ? (
                    <a
                      href={schedule.meetingUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="inline-flex items-center gap-1 text-xs font-semibold text-sky-600 underline hover:text-sky-700 dark:text-sky-400"
                    >
                      <span>Join Meeting</span>
                      <ExternalLink className="size-4" />
                    </a>
                  ) : (
                    <p className="text-xs text-neutral-400 italic">
                      {t("sharing.meetingUrlNotice")}
                    </p>
                  )}
                </div>
              </div>
            )}

            {/* Audience Scope */}
            <div className="flex items-start gap-3 text-sm">
              <Users className="mt-0.5 size-6 shrink-0 text-neutral-400" />
              <div>
                <p className="font-semibold text-neutral-900 dark:text-white">
                  {schedule.audienceScope === "AllMembers"
                    ? t("sharing.allMembers")
                    : t("sharing.selectedAudience")}
                </p>
              </div>
            </div>
          </div>

          {/* Presenters */}
          {schedule.presenters && schedule.presenters.length > 0 && (
            <div>
              <h4 className="text-md mb-3 font-bold tracking-wider text-neutral-800 dark:text-zinc-500">
                {t("sharing.presenters")}
              </h4>
              <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2">
                {schedule.presenters.map((p) => (
                  <div
                    key={p.userId}
                    className="flex items-center gap-3 rounded-xl border border-neutral-200 bg-white p-3 dark:border-zinc-800 dark:bg-zinc-900"
                  >
                    <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-blue-50 text-xs font-bold text-blue-600 dark:bg-blue-950/60 dark:text-blue-400">
                      {p.fullName.charAt(0).toUpperCase()}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-semibold text-neutral-900 dark:text-white">
                        {p.fullName}
                      </p>
                      <Badge
                        variant="secondary"
                        className="mt-0.5 py-0 text-[12px]"
                      >
                        {p.role}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Linked Contents */}
          {schedule.contents && schedule.contents.length > 0 && (
            <div>
              <h4 className="mb-3 text-xs font-bold tracking-wider text-neutral-400 uppercase dark:text-zinc-500">
                {t("sharing.linkedContents")}
              </h4>
              <div className="space-y-2">
                {schedule.contents.map((c) => (
                  <a
                    key={c.id}
                    href={`/sharing/${c.slug}`}
                    className="flex items-center justify-between rounded-xl border border-neutral-200 bg-white p-3 text-sm font-semibold text-neutral-800 hover:border-neutral-300 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-200"
                  >
                    <div className="flex min-w-0 items-center gap-2">
                      <BookOpen className="size-4 shrink-0 text-blue-500" />
                      <span className="truncate">{c.title}</span>
                    </div>
                    <ExternalLink className="size-3.5 shrink-0 text-neutral-400" />
                  </a>
                ))}
              </div>
            </div>
          )}

          {/* Footer Actions */}
          <DialogFooter className="flex items-center justify-between border-t border-neutral-100 pt-4 dark:border-zinc-800">
            <div className="flex items-center gap-2">
              {canManage && schedule.status !== "Cancelled" && (
                <>
                  <Button
                    variant="brand"
                    onClick={() => {
                      onClose();
                      onEdit?.(schedule);
                    }}
                  >
                    <FileEdit className="mr-1.5 size-3.5" />
                    <span>{t("sharing.editSchedule")}</span>
                  </Button>
                  <Button
                    variant="destructive"
                    onClick={() => {
                      onClose();
                      onCancel?.(schedule);
                    }}
                  >
                    <XCircle className="mr-1.5 size-3.5" />
                    <span>{t("sharing.cancelSchedule")}</span>
                  </Button>
                </>
              )}
            </div>

            <Button variant="outline" onClick={onClose}>
              Close
            </Button>
          </DialogFooter>
        </div>
      </DialogContent>
    </Dialog>
  );
}
