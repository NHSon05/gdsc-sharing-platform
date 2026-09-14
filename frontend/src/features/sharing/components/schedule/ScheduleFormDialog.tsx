"use client";

import React, { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { TextField } from "@/components/ui/input";
import { useTranslation } from "@/core/i18n/i18n.context";
import {
  useCreateScheduleMutation,
  useUpdateScheduleMutation,
  useCreateAdminScheduleMutation,
  useUpdateAdminScheduleMutation,
} from "../../hooks/use-schedule-mutations";
import { useContentsQuery } from "../../hooks/use-contents-query";
import type {
  ScheduleRequest,
  ScheduleResponse,
  SharingType,
  DeliveryMode,
  AudienceScope,
} from "../../types/sharing.types";
import { AlertCircle, Loader2 } from "lucide-react";

interface ScheduleFormDialogProps {
  isOpen: boolean;
  onClose: () => void;
  initialData?: ScheduleResponse | null;
  isAdmin?: boolean;
  onSuccess?: (schedule: ScheduleResponse) => void;
}

const SHARING_TYPES: SharingType[] = [
  "Workshop",
  "TechTalk",
  "PanelDiscussion",
  "InternalSharing",
];

const DELIVERY_MODES: DeliveryMode[] = ["Offline", "Online", "Hybrid"];

function getDefaultDates(initialData?: ScheduleResponse | null) {
  if (initialData) {
    const start = new Date(initialData.startsAtUtc);
    const end = new Date(initialData.endsAtUtc);
    return {
      startDate: start.toISOString().split("T")[0],
      startTime: start.toTimeString().slice(0, 5),
      endDate: end.toISOString().split("T")[0],
      endTime: end.toTimeString().slice(0, 5),
    };
  }
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const dateStr = tomorrow.toISOString().split("T")[0];
  return {
    startDate: dateStr,
    startTime: "09:00",
    endDate: dateStr,
    endTime: "11:00",
  };
}

interface ScheduleFormInnerProps {
  initialData?: ScheduleResponse | null;
  isAdmin?: boolean;
  onClose: () => void;
  onSuccess?: (schedule: ScheduleResponse) => void;
}

function ScheduleFormInner({
  initialData,
  isAdmin = false,
  onClose,
  onSuccess,
}: ScheduleFormInnerProps) {
  const { t } = useTranslation();

  const createMemberMutation = useCreateScheduleMutation();
  const updateMemberMutation = useUpdateScheduleMutation();
  const createAdminMutation = useCreateAdminScheduleMutation();
  const updateAdminMutation = useUpdateAdminScheduleMutation();

  const { data: contentsData } = useContentsQuery({ pageSize: 50 });
  const contents = contentsData?.items || [];

  const isEditing = Boolean(initialData);
  const defaultDates = getDefaultDates(initialData);

  const [title, setTitle] = useState(initialData?.title ?? "");
  const [description, setDescription] = useState(
    initialData?.description ?? ""
  );
  const [sharingType, setSharingType] = useState<SharingType>(
    initialData?.sharingType ?? "TechTalk"
  );
  const [deliveryMode, setDeliveryMode] = useState<DeliveryMode>(
    initialData?.deliveryMode ?? "Offline"
  );
  const [startsAtDate, setStartsAtDate] = useState(defaultDates.startDate);
  const [startsAtTime, setStartsAtTime] = useState(defaultDates.startTime);
  const [endsAtDate, setEndsAtDate] = useState(defaultDates.endDate);
  const [endsAtTime, setEndsAtTime] = useState(defaultDates.endTime);
  const [timeZoneId, setTimeZoneId] = useState(
    initialData?.timeZoneId ?? "Asia/Ho_Chi_Minh"
  );
  const [location, setLocation] = useState(
    initialData?.location ?? "Phòng Lab GDSC"
  );
  const [meetingUrl, setMeetingUrl] = useState(initialData?.meetingUrl ?? "");
  const [audienceScope, setAudienceScope] = useState<AudienceScope>(
    initialData?.audienceScope ?? "AllMembers"
  );
  const [selectedContentIds, setSelectedContentIds] = useState<string[]>(
    initialData?.contents?.map((c) => c.id) ?? []
  );

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!title.trim()) {
      setErrorMessage("Title is required.");
      return;
    }
    if (!startsAtDate || !startsAtTime || !endsAtDate || !endsAtTime) {
      setErrorMessage("Please specify start and end dates and times.");
      return;
    }

    const startsAtLocal = `${startsAtDate}T${startsAtTime}:00`;
    const endsAtLocal = `${endsAtDate}T${endsAtTime}:00`;

    const payload: ScheduleRequest = {
      title: title.trim(),
      description: description.trim() || null,
      sharingType,
      deliveryMode,
      startsAtLocal,
      endsAtLocal,
      timeZoneId,
      audienceScope,
      presenters: [],
      contentIds: selectedContentIds,
      generationIds: [],
      departmentIds: [],
      location: deliveryMode === "Online" ? null : location.trim() || null,
      meetingUrl: deliveryMode === "Offline" ? null : meetingUrl.trim() || null,
    };

    try {
      if (isEditing && initialData) {
        let result: ScheduleResponse;
        if (isAdmin) {
          result = await updateAdminMutation.mutateAsync({
            id: initialData.id,
            request: payload,
            version: initialData.version,
          });
        } else {
          result = await updateMemberMutation.mutateAsync({
            id: initialData.id,
            request: payload,
            version: initialData.version,
          });
        }
        onSuccess?.(result);
      } else {
        let result: ScheduleResponse;
        if (isAdmin) {
          result = await createAdminMutation.mutateAsync(payload);
        } else {
          result = await createMemberMutation.mutateAsync(payload);
        }
        onSuccess?.(result);
      }
      onClose();
    } catch (err: unknown) {
      const apiErr = err as { status?: number; message?: string } | undefined;
      if (apiErr?.status === 412) {
        setErrorMessage(t("sharing.concurrencyError"));
      } else if (apiErr?.status === 409) {
        setErrorMessage(
          apiErr?.message ||
            "Conflict detected: Presenter has another overlapping session during this timeframe."
        );
      } else {
        setErrorMessage(
          apiErr?.message || "An error occurred while saving the schedule."
        );
      }
    }
  };

  const isPending =
    createMemberMutation.isPending ||
    updateMemberMutation.isPending ||
    createAdminMutation.isPending ||
    updateAdminMutation.isPending;

  return (
    <>
      <DialogHeader>
        <DialogTitle className="text-xl font-bold text-neutral-900 dark:text-white">
          {isEditing ? t("sharing.editSchedule") : t("sharing.createSchedule")}
        </DialogTitle>
      </DialogHeader>

      {errorMessage && (
        <div className="flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs font-semibold text-rose-700 dark:border-rose-900/60 dark:bg-rose-950/40 dark:text-rose-300">
          <AlertCircle className="size-4 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Title */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.contentTitle")} *
          </label>
          <TextField
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. React Performance & Architecture"
            required
          />
        </div>

        {/* Description */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.summary")}
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Details about this sharing session..."
            rows={2}
            className="w-full rounded-xl border border-neutral-200 bg-white p-3 text-sm text-neutral-900 placeholder:text-neutral-400 focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-white dark:placeholder:text-zinc-500"
          />
        </div>

        {/* Sharing Type & Delivery Mode */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.sharingType")}
            </label>
            <select
              value={sharingType}
              onChange={(e) => setSharingType(e.target.value as SharingType)}
              className="h-10 w-full rounded-xl border border-neutral-200 bg-white px-3 text-sm font-medium text-neutral-700 shadow-2xs focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
            >
              {SHARING_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.deliveryMode")}
            </label>
            <select
              value={deliveryMode}
              onChange={(e) => setDeliveryMode(e.target.value as DeliveryMode)}
              className="h-10 w-full rounded-xl border border-neutral-200 bg-white px-3 text-sm font-medium text-neutral-700 shadow-2xs focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
            >
              {DELIVERY_MODES.map((mode) => (
                <option key={mode} value={mode}>
                  {mode}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Conditional Location & Meeting URL */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {deliveryMode !== "Online" && (
            <div>
              <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                {t("sharing.location")}
              </label>
              <TextField
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                placeholder="e.g. Phòng Lab 01"
              />
            </div>
          )}

          {deliveryMode !== "Offline" && (
            <div>
              <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                {t("sharing.meetingUrl")}
              </label>
              <TextField
                value={meetingUrl}
                onChange={(e) => setMeetingUrl(e.target.value)}
                placeholder="https://meet.google.com/xyz-abc-def"
              />
            </div>
          )}
        </div>

        {/* Start and End Times */}
        <div className="space-y-3 rounded-xl border border-neutral-200 bg-neutral-50/60 p-4 dark:border-zinc-800 dark:bg-zinc-900/40">
          <h4 className="text-xs font-bold tracking-wider text-neutral-500 uppercase dark:text-zinc-400">
            {t("sharing.scheduleTime")}
          </h4>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-xs font-medium text-neutral-600 dark:text-zinc-400">
                {t("sharing.startsAt")} *
              </label>
              <div className="flex gap-2">
                <input
                  type="date"
                  value={startsAtDate}
                  onChange={(e) => setStartsAtDate(e.target.value)}
                  className="h-10 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs text-neutral-900 dark:border-zinc-800 dark:bg-zinc-900 dark:text-white"
                  required
                />
                <input
                  type="time"
                  value={startsAtTime}
                  onChange={(e) => setStartsAtTime(e.target.value)}
                  className="h-10 w-28 rounded-xl border border-neutral-200 bg-white px-2 text-xs text-neutral-900 dark:border-zinc-800 dark:bg-zinc-900 dark:text-white"
                  required
                />
              </div>
            </div>

            <div>
              <label className="mb-1 block text-xs font-medium text-neutral-600 dark:text-zinc-400">
                {t("sharing.endsAt")} *
              </label>
              <div className="flex gap-2">
                <input
                  type="date"
                  value={endsAtDate}
                  onChange={(e) => setEndsAtDate(e.target.value)}
                  className="h-10 w-full rounded-xl border border-neutral-200 bg-white px-3 text-xs text-neutral-900 dark:border-zinc-800 dark:bg-zinc-900 dark:text-white"
                  required
                />
                <input
                  type="time"
                  value={endsAtTime}
                  onChange={(e) => setEndsAtTime(e.target.value)}
                  className="h-10 w-28 rounded-xl border border-neutral-200 bg-white px-2 text-xs text-neutral-900 dark:border-zinc-800 dark:bg-zinc-900 dark:text-white"
                  required
                />
              </div>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-neutral-600 dark:text-zinc-400">
              {t("sharing.timezone")} (IANA identifier)
            </label>
            <TextField
              value={timeZoneId}
              onChange={(e) => setTimeZoneId(e.target.value)}
              placeholder="Asia/Ho_Chi_Minh"
              required
            />
          </div>
        </div>

        {/* Audience Scope */}
        <div>
          <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
            {t("sharing.audience")}
          </label>
          <select
            value={audienceScope}
            onChange={(e) => setAudienceScope(e.target.value as AudienceScope)}
            className="h-10 w-full rounded-xl border border-neutral-200 bg-white px-3 text-sm font-medium text-neutral-700 shadow-2xs focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
          >
            <option value="AllMembers">{t("sharing.allMembers")}</option>
            <option value="SelectedAudience">
              {t("sharing.selectedAudience")}
            </option>
          </select>
        </div>

        {/* Linked Content Selection */}
        {contents.length > 0 && (
          <div>
            <label className="mb-1 block text-xs font-semibold text-neutral-700 dark:text-zinc-300">
              {t("sharing.linkedContents")}
            </label>
            <select
              multiple
              value={selectedContentIds}
              onChange={(e) => {
                const values = Array.from(
                  e.target.selectedOptions,
                  (option) => option.value
                );
                setSelectedContentIds(values);
              }}
              className="h-24 w-full rounded-xl border border-neutral-200 bg-white p-2 text-xs text-neutral-700 shadow-2xs focus:border-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
            >
              {contents.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.title}
                </option>
              ))}
            </select>
            <span className="text-[11px] text-neutral-400 dark:text-zinc-500">
              Hold Ctrl (Cmd on Mac) to select multiple contents.
            </span>
          </div>
        )}

        <DialogFooter className="flex justify-end gap-2 border-t border-neutral-100 pt-4 dark:border-zinc-800">
          <Button
            type="button"
            variant="outline"
            onClick={onClose}
            disabled={isPending}
          >
            Cancel
          </Button>
          <Button type="submit" variant="brand" disabled={isPending}>
            {isPending && <Loader2 className="mr-2 size-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Create Schedule"}
          </Button>
        </DialogFooter>
      </form>
    </>
  );
}

export function ScheduleFormDialog({
  isOpen,
  onClose,
  initialData,
  isAdmin = false,
  onSuccess,
}: ScheduleFormDialogProps) {
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="max-h-[90vh] max-w-2xl overflow-y-auto p-6">
        {isOpen && (
          <ScheduleFormInner
            key={initialData?.id ?? "new-schedule"}
            initialData={initialData}
            isAdmin={isAdmin}
            onClose={onClose}
            onSuccess={onSuccess}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
